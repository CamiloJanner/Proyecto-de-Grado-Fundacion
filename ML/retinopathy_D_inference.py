import argparse
import json
import math
import os
import sys

import numpy as np
import torch
import torch.nn as nn
from PIL import Image


POSITIVE_LABEL = "Retinopatía Diabética"
NEGATIVE_LABEL = "Sin signos de retinopatía"


class Permute(nn.Module):
    def __init__(self, dims):
        super().__init__()
        self.dims = dims

    def forward(self, x):
        return x.permute(*self.dims)


class LayerNorm2d(nn.LayerNorm):
    def __init__(self, num_channels, eps=1e-6):
        super().__init__(num_channels, eps=eps)

    def forward(self, x):
        x = x.permute(0, 2, 3, 1)
        x = super().forward(x)
        x = x.permute(0, 3, 1, 2)
        return x


class CNBlock(nn.Module):
    def __init__(self, dim, layer_scale=1e-6):
        super().__init__()

        self.block = nn.Sequential(
            nn.Conv2d(dim, dim, kernel_size=7, padding=3, groups=dim, bias=True),
            Permute((0, 2, 3, 1)),
            nn.LayerNorm(dim, eps=1e-6),
            nn.Linear(dim, 4 * dim, bias=True),
            nn.GELU(),
            nn.Linear(4 * dim, dim, bias=True),
            Permute((0, 3, 1, 2)),
        )

        self.layer_scale = nn.Parameter(torch.ones(dim, 1, 1) * layer_scale)

    def forward(self, x):
        return x + self.layer_scale * self.block(x)


class ConvNeXtTinyBinary(nn.Module):
    def __init__(self, num_classes=1):
        super().__init__()

        settings = [(96, 3), (192, 3), (384, 9), (768, 3)]

        layers = [
            nn.Sequential(
                nn.Conv2d(3, settings[0][0], kernel_size=4, stride=4, padding=0, bias=True),
                LayerNorm2d(settings[0][0], eps=1e-6),
            )
        ]

        for i, (channels, depth) in enumerate(settings):
            stage = [CNBlock(channels, layer_scale=1e-6) for _ in range(depth)]
            layers.append(nn.Sequential(*stage))

            if i != len(settings) - 1:
                next_channels = settings[i + 1][0]
                layers.append(
                    nn.Sequential(
                        LayerNorm2d(channels, eps=1e-6),
                        nn.Conv2d(channels, next_channels, kernel_size=2, stride=2),
                    )
                )

        self.features = nn.Sequential(*layers)
        self.avgpool = nn.AdaptiveAvgPool2d(1)

        # Se conserva la estructura de índices del state_dict:
        # classifier.0 -> LayerNorm
        # classifier.1 -> Flatten
        # classifier.2 -> Linear
        self.classifier = nn.Sequential(
            nn.LayerNorm(768, eps=1e-6),
            nn.Flatten(1),
            nn.Linear(768, num_classes),
        )

    def forward(self, x):
        x = self.features(x)
        x = self.avgpool(x)
        x = x.flatten(1)
        x = self.classifier[0](x)
        x = self.classifier[2](x)
        return x


def preprocess_image(image_path: str) -> torch.Tensor:
    image = Image.open(image_path).convert("RGB")
    image = image.resize((224, 224))

    arr = np.asarray(image).astype("float32") / 255.0

    mean = np.array([0.485, 0.456, 0.406], dtype="float32")
    std = np.array([0.229, 0.224, 0.225], dtype="float32")

    arr = (arr - mean) / std
    tensor = torch.from_numpy(arr).permute(2, 0, 1).unsqueeze(0)

    return tensor


def run_inference(model_path: str, image_path: str):
    torch.set_num_threads(max(1, os.cpu_count() // 2))

    model = ConvNeXtTinyBinary(num_classes=1)
    state_dict = torch.load(model_path, map_location="cpu")
    model.load_state_dict(state_dict, strict=True)
    model.eval()

    x = preprocess_image(image_path)

    with torch.no_grad():
        logit = model(x).squeeze().item()

    probability_positive = 1.0 / (1.0 + math.exp(-logit))
    probability_negative = 1.0 - probability_positive

    if probability_positive >= 0.5:
        label = POSITIVE_LABEL
        confidence = probability_positive
    else:
        label = NEGATIVE_LABEL
        confidence = probability_negative

    return {
        "success": True,
        "label": label,
        "confidence": round(confidence * 100, 2),
        "probability_positive": round(probability_positive * 100, 2),
        "probability_negative": round(probability_negative * 100, 2),
        "logit": round(float(logit), 6),
    }


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--model", required=True)
    parser.add_argument("--image", required=True)
    args = parser.parse_args()

    try:
        result = run_inference(args.model, args.image)
        print(json.dumps(result, ensure_ascii=False))
    except Exception as ex:
        print(json.dumps({
            "success": False,
            "message": str(ex)
        }, ensure_ascii=False))
        sys.exit(1)


if __name__ == "__main__":
    main()