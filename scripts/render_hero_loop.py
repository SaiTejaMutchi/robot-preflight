import os
import cv2
import numpy as np
from PIL import Image, ImageDraw, ImageFont

# Canvas dimensions
WIDTH, HEIGHT = 1600, 900
FPS = 15
TOTAL_FRAMES = 120  # 8.0 seconds at 15 fps

# Fonts
FONT_HELV_XL = ImageFont.truetype('/System/Library/Fonts/HelveticaNeue.ttc', 38)
FONT_HELV_LG = ImageFont.truetype('/System/Library/Fonts/HelveticaNeue.ttc', 24)
FONT_HELV_MD = ImageFont.truetype('/System/Library/Fonts/HelveticaNeue.ttc', 17)
FONT_HELV_SM = ImageFont.truetype('/System/Library/Fonts/HelveticaNeue.ttc', 13)
FONT_HELV_XS = ImageFont.truetype('/System/Library/Fonts/HelveticaNeue.ttc', 11)

FONT_MONO_XL = ImageFont.truetype('/System/Library/Fonts/Menlo.ttc', 34)
FONT_MONO_LG = ImageFont.truetype('/System/Library/Fonts/Menlo.ttc', 24)
FONT_MONO_MD = ImageFont.truetype('/System/Library/Fonts/Menlo.ttc', 18)
FONT_MONO_SM = ImageFont.truetype('/System/Library/Fonts/Menlo.ttc', 14)
FONT_MONO_XS = ImageFont.truetype('/System/Library/Fonts/Menlo.ttc', 11)

# Prepare base warehouse images
im_clean_cv = cv2.imread('scratch/f3_inpainted.png')
im_clean_cv = cv2.resize(im_clean_cv, (WIDTH, HEIGHT))
# Neutralize top chrome bar with ceiling color
im_clean_cv[0:55, :] = im_clean_cv[55:60, :].mean(axis=0).astype(np.uint8)

im_meas_cv = cv2.imread('media/screenshots/03_measurement_detail.png')
im_meas_cv = cv2.resize(im_meas_cv, (WIDTH, HEIGHT))
im_meas_cv[0:55, :] = im_meas_cv[55:60, :].mean(axis=0).astype(np.uint8)

# Cyan line & highlight mask
b, g, r = cv2.split(im_meas_cv)
cyan_mask = ((g > 150) & (b > 150) & (r < 115)).astype(np.float32)
cyan_mask_smooth = cv2.GaussianBlur(cyan_mask, (3, 3), 0)

# Load real spec sheet crop
spec_crop = Image.open('scratch/spec_crop.png')
# Resize spec crop slightly to fit neatly into requirement card
# Original is approx 320x60
spec_crop_resized = spec_crop.resize((360, int(spec_crop.height * (360 / spec_crop.width))), Image.LANCZOS)

# Create output frame directory
os.makedirs('scratch/frames', exist_ok=True)

def draw_card(draw, box, bg_color, border_color, radius=8, width=1):
    draw.rounded_rectangle(box, radius=radius, fill=bg_color, outline=border_color, width=width)

print("Starting frame generation...")

for frame_idx in range(TOTAL_FRAMES):
    t = frame_idx / FPS  # time in seconds (0.0 to 8.0)

    # 1. Base Plate compositing (line reveal from left to right)
    # Between 2.4s and 4.0s (frames 36 to 59), the real Unity line reveals from x=100 to x=1200
    if frame_idx < 36:
        frame_cv = im_clean_cv.copy()
    elif frame_idx < 60:
        # Progress 0.0 to 1.0
        prog = (frame_idx - 36) / 23.0
        # Ease out
        prog_eased = 1.0 - (1.0 - prog) ** 2
        reveal_x = 100 + prog_eased * (1180 - 100)
        # Create horizontal reveal mask
        reveal_mask = np.zeros_like(cyan_mask_smooth)
        reveal_mask[:, :int(reveal_x)] = 1.0
        active_mask = cyan_mask_smooth * reveal_mask
        active_mask_3c = np.repeat(active_mask[:, :, np.newaxis], 3, axis=2)
        frame_cv = (im_clean_cv * (1.0 - active_mask_3c) + im_meas_cv * active_mask_3c).astype(np.uint8)
    else:
        # Frame 60 to 112: Full measured plate
        if frame_idx < 112:
            frame_cv = im_meas_cv.copy()
        else:
            # Fade back to clean plate (112 to 119)
            fade = (119 - frame_idx) / 7.0
            frame_cv = (im_clean_cv * (1.0 - fade) + im_meas_cv * fade).astype(np.uint8)

    # Convert to PIL Image RGBA for high-quality alpha graphics
    img_rgb = cv2.cvtColor(frame_cv, cv2.COLOR_BGR2RGB)
    img = Image.fromarray(img_rgb).convert('RGBA')
    overlay = Image.new('RGBA', (WIDTH, HEIGHT), (0, 0, 0, 0))
    draw = ImageDraw.Draw(overlay)

    # Master loop fade for overlays (fade out in last 8 frames)
    master_alpha = 1.0
    if frame_idx >= 112:
        master_alpha = max(0.0, (119 - frame_idx) / 7.0)

    # =========================================================================
    # OVERLAY 1: Top-Left Engineering Badge (Active from frame 0 onwards)
    # =========================================================================
    alpha_tl = min(1.0, (frame_idx + 1) / 8.0) * master_alpha
    if alpha_tl > 0:
        bg_a = int(220 * alpha_tl)
        border_a = int(255 * alpha_tl)
        text_a = int(255 * alpha_tl)
        text_dim_a = int(180 * alpha_tl)
        accent_a = int(255 * alpha_tl)

        # Header card
        draw_card(draw, [40, 32, 480, 114], (11, 17, 28, bg_a), (30, 48, 75, border_a), radius=8, width=1)
        # Dot indicator
        draw.ellipse([58, 48, 68, 58], fill=(0, 229, 200, accent_a))
        draw.text((76, 46), "FACILITY GEOMETRY PREFLIGHT", fill=(148, 163, 184, text_dim_a), font=FONT_MONO_XS)
        draw.text((58, 68), "AWS RoboMaker Small Warehouse", fill=(248, 250, 252, text_a), font=FONT_HELV_MD)
        draw.text((58, 92), "TARGET ROBOT: OTTO 1500 AMR", fill=(0, 229, 200, accent_a), font=FONT_MONO_XS)

    # =========================================================================
    # OVERLAY 2: Top-Right Requirement Card (Fades in at 1.3s = frame 20)
    # =========================================================================
    if frame_idx >= 20:
        alpha_req = min(1.0, (frame_idx - 20) / 8.0) * master_alpha
        if alpha_req > 0:
            bg_a = int(235 * alpha_req)
            border_a = int(255 * alpha_req)
            text_a = int(255 * alpha_req)
            text_dim_a = int(180 * alpha_req)
            cyan_a = int(255 * alpha_req)

            card_x1, card_y1, card_x2, card_y2 = 1110, 32, 1560, 260
            draw_card(draw, [card_x1, card_y1, card_x2, card_y2], (11, 17, 28, bg_a), (30, 58, 90, border_a), radius=8, width=2)

            # Header strip inside card
            draw.rounded_rectangle([card_x1, card_y1, card_x2, card_y1 + 36], radius=8, fill=(19, 35, 61, int(240 * alpha_req)))
            draw.rectangle([card_x1, card_y1 + 28, card_x2, card_y1 + 36], fill=(19, 35, 61, int(240 * alpha_req)))
            draw.line([card_x1, card_y1 + 36, card_x2, card_y1 + 36], fill=(30, 58, 90, border_a), width=1)
            draw.text((card_x1 + 18, card_y1 + 10), "ROBOT REQUIREMENT", fill=(56, 189, 248, cyan_a), font=FONT_MONO_XS)
            draw.text((card_x2 - 18, card_y1 + 10), "OTTO-1500", fill=(148, 163, 184, text_dim_a), font=FONT_MONO_XS, anchor="ra")

            # Requirement fields
            draw.text((card_x1 + 18, card_y1 + 48), "Constraint:", fill=(148, 163, 184, text_dim_a), font=FONT_HELV_XS)
            draw.text((card_x1 + 18, card_y1 + 64), "Minimum one-way aisle width", fill=(248, 250, 252, text_a), font=FONT_HELV_MD)

            draw.text((card_x1 + 18, card_y1 + 92), "Required Value:", fill=(148, 163, 184, text_dim_a), font=FONT_HELV_XS)
            draw.text((card_x1 + 18, card_y1 + 108), "1.915 m", fill=(255, 255, 255, text_a), font=FONT_MONO_LG)
            draw.text((card_x1 + 155, card_y1 + 116), "(1,915 mm / 78 in)", fill=(148, 163, 184, text_dim_a), font=FONT_MONO_XS)

            # Spec sheet source label
            draw.text((card_x1 + 18, card_y1 + 148), "MFR SPECIFICATION PROVENANCE (PAGE 1 CROP):", fill=(100, 116, 139, text_dim_a), font=FONT_MONO_XS)

            # Paste real spec sheet crop into overlay
            crop_box = [card_x1 + 18, card_y1 + 168]
            # Convert spec crop to RGBA with alpha
            crop_rgba = spec_crop_resized.convert('RGBA')
            if alpha_req < 1.0:
                crop_arr = np.array(crop_rgba)
                crop_arr[:, :, 3] = (crop_arr[:, :, 3].astype(float) * alpha_req).astype(np.uint8)
                crop_rgba = Image.fromarray(crop_arr)

            # Draw white backplate for spec crop
            draw_card(draw, [card_x1 + 16, card_y1 + 166, card_x1 + 16 + 364, card_y1 + 166 + crop_rgba.height + 4],
                      (255, 255, 255, int(240 * alpha_req)), (200, 200, 200, border_a), radius=4, width=1)
            overlay.paste(crop_rgba, (card_x1 + 18, card_y1 + 168), crop_rgba)

    # =========================================================================
    # OVERLAY 3: Shelf Boundary Markers (Fades in at 2.4s = frame 36)
    # =========================================================================
    if frame_idx >= 36:
        alpha_bnd = min(1.0, (frame_idx - 36) / 6.0) * master_alpha
        if alpha_bnd > 0:
            bg_a = int(220 * alpha_bnd)
            border_a = int(255 * alpha_bnd)
            cyan_a = int(255 * alpha_bnd)
            text_a = int(255 * alpha_bnd)

            # Boundary A (Shelf F on left, x ~ 107 in 1600w)
            bx_a = 107
            draw.line([bx_a, 410, bx_a, 760], fill=(0, 229, 200, int(180 * alpha_bnd)), width=2)
            draw_card(draw, [28, 350, 250, 404], (11, 17, 28, bg_a), (0, 229, 200, border_a), radius=6, width=1)
            draw.text((40, 358), "BOUNDARY A (WEST FACE)", fill=(0, 229, 200, cyan_a), font=FONT_MONO_XS)
            draw.text((40, 376), "aws_..._ShelfF_01_001", fill=(248, 250, 252, text_a), font=FONT_MONO_SM)

            # Boundary B (Shelf D on right, x ~ 1176 in 1600w)
            bx_b = 1176
            draw.line([bx_b, 410, bx_b, 760], fill=(0, 229, 200, int(180 * alpha_bnd)), width=2)
            draw_card(draw, [1030, 350, 1252, 404], (11, 17, 28, bg_a), (0, 229, 200, border_a), radius=6, width=1)
            draw.text((1042, 358), "BOUNDARY B (EAST FACE)", fill=(0, 229, 200, cyan_a), font=FONT_MONO_XS)
            draw.text((1042, 376), "aws_..._ShelfD_01_001", fill=(248, 250, 252, text_a), font=FONT_MONO_SM)

    # =========================================================================
    # OVERLAY 4: Measurement Badge (Fades in at 4.0s = frame 60)
    # =========================================================================
    if 60 <= frame_idx < 81:
        alpha_meas = min(1.0, (frame_idx - 60) / 6.0) * master_alpha
        if alpha_meas > 0:
            bg_a = int(240 * alpha_meas)
            border_a = int(255 * alpha_meas)
            cyan_a = int(255 * alpha_meas)
            text_a = int(255 * alpha_meas)

            mx1, my1, mx2, my2 = 530, 480, 1070, 580
            draw_card(draw, [mx1, my1, mx2, my2], (9, 24, 38, bg_a), (0, 229, 200, border_a), radius=8, width=2)
            draw.text((641, my1 + 14), "DETERMINISTIC MEASURED CLEAR SPAN", fill=(148, 163, 184, int(200 * alpha_meas)), font=FONT_MONO_XS)
            draw.text((615, my1 + 32), "7.509663 m", fill=(0, 229, 200, cyan_a), font=FONT_MONO_XL)
            draw.text((605, my1 + 72), "Axis: X  ·  Nearest-Face Bounds Gap (Unity)", fill=(203, 213, 225, text_a), font=FONT_MONO_XS)

    # =========================================================================
    # OVERLAY 5: Calculation Arithmetic Card (Fades in at 5.4s = frame 81)
    # =========================================================================
    if 81 <= frame_idx < 98:
        alpha_calc = min(1.0, (frame_idx - 81) / 5.0) * master_alpha
        if alpha_calc > 0:
            bg_a = int(245 * alpha_calc)
            border_a = int(255 * alpha_calc)
            cyan_a = int(255 * alpha_calc)
            green_a = int(255 * alpha_calc)
            text_a = int(255 * alpha_calc)
            dim_a = int(180 * alpha_calc)

            cx1, cy1, cx2, cy2 = 520, 450, 1080, 620
            draw_card(draw, [cx1, cy1, cx2, cy2], (10, 22, 35, bg_a), (0, 229, 200, border_a), radius=8, width=2)

            draw.text((cx1 + 24, cy1 + 16), "CLEARANCE MARGIN ARITHMETIC", fill=(56, 189, 248, cyan_a), font=FONT_MONO_XS)

            # Measured row
            draw.text((cx1 + 40, cy1 + 42), "  7.509663 m", fill=(0, 229, 200, cyan_a), font=FONT_MONO_MD)
            draw.text((cx1 + 220, cy1 + 44), "(measured aisle clearance)", fill=(148, 163, 184, dim_a), font=FONT_HELV_SM)

            # Required row
            draw.text((cx1 + 40, cy1 + 72), "− 1.915000 m", fill=(248, 250, 252, text_a), font=FONT_MONO_MD)
            draw.text((cx1 + 220, cy1 + 74), "(required minimum aisle width)", fill=(148, 163, 184, dim_a), font=FONT_HELV_SM)

            # Divider line
            draw.line([cx1 + 36, cy1 + 104, cx2 - 36, cy1 + 104], fill=(51, 65, 85, border_a), width=2)

            # Margin row
            draw.text((cx1 + 40, cy1 + 116), "= +5.594663 m", fill=(52, 211, 153, green_a), font=FONT_MONO_LG)
            draw.text((cx1 + 270, cy1 + 122), "CLEARANCE MARGIN", fill=(52, 211, 153, green_a), font=FONT_MONO_SM)

    # =========================================================================
    # OVERLAY 6: Final PASS Decision Card (Fades in at 6.5s = frame 98)
    # =========================================================================
    if frame_idx >= 98:
        alpha_pass = min(1.0, (frame_idx - 98) / 5.0) * master_alpha
        if alpha_pass > 0:
            bg_a = int(248 * alpha_pass)
            border_a = int(255 * alpha_pass)
            green_a = int(255 * alpha_pass)
            text_a = int(255 * alpha_pass)
            dim_a = int(180 * alpha_pass)

            px1, py1, px2, py2 = 460, 420, 1140, 650
            draw_card(draw, [px1, py1, px2, py2], (6, 32, 24, bg_a), (16, 185, 129, border_a), radius=10, width=2)

            # PASS badge pill
            draw_card(draw, [px1 + 24, py1 + 22, px1 + 180, py1 + 84], (5, 46, 33, bg_a), (16, 185, 129, border_a), radius=6, width=2)
            draw.text((px1 + 44, py1 + 32), "PASS", fill=(52, 211, 153, green_a), font=FONT_MONO_XL)

            # Subtitles next to PASS badge
            draw.text((px1 + 204, py1 + 30), "aisle clearance verified", fill=(255, 255, 255, text_a), font=FONT_HELV_LG)
            draw.text((px1 + 204, py1 + 60), "selected aisle constraint", fill=(110, 231, 183, green_a), font=FONT_MONO_SM)

            # Divider line
            draw.line([px1 + 24, py1 + 104, px2 - 24, py1 + 104], fill=(16, 75, 53, border_a), width=1)

            # Detail summary columns
            col1_x = px1 + 30
            draw.text((col1_x, py1 + 120), "REQUIRED", fill=(148, 163, 184, dim_a), font=FONT_MONO_XS)
            draw.text((col1_x, py1 + 138), "1.915000 m", fill=(248, 250, 252, text_a), font=FONT_MONO_SM)

            col2_x = px1 + 230
            draw.text((col2_x, py1 + 120), "MEASURED", fill=(148, 163, 184, dim_a), font=FONT_MONO_XS)
            draw.text((col2_x, py1 + 138), "7.509663 m", fill=(0, 229, 200, green_a), font=FONT_MONO_SM)

            col3_x = px1 + 430
            draw.text((col3_x, py1 + 120), "CLEARANCE MARGIN", fill=(148, 163, 184, dim_a), font=FONT_MONO_XS)
            draw.text((col3_x, py1 + 138), "+5.594663 m", fill=(52, 211, 153, green_a), font=FONT_MONO_SM)

            # Bottom provenance note
            draw.text((px1 + 30, py1 + 184), "Deterministic glTF bounds check · agrees with independent Python cross-check within 1 µm",
                      fill=(110, 231, 183, int(200 * alpha_pass)), font=FONT_MONO_XS)

    # Composite overlay onto base image
    final_img = Image.alpha_composite(img, overlay).convert('RGB')
    final_img.save(f'scratch/frames/frame_{frame_idx:03d}.png')

print("All 120 frames rendered successfully.")
