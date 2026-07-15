#include "include/core/SkBitmap.h"
#include "include/core/SkCanvas.h"
#include "include/core/SkImage.h"
#include "include/core/SkPaint.h"
#include "include/core/SkRect.h"
#include "include/core/SkSurface.h"
#include "include/core/SkData.h"
#include "include/core/SkFont.h"
#include "include/core/SkTypeface.h"
#include <chrono>
#include <iostream>
#include <vector>

const int CANVAS_WIDTH = 1080;
const int CANVAS_height = 2400;
const int NUM_ICONS = 10;
const int ITERATIONS = 100;

sk_sp<SkImage> load_image(const char* path) {
    auto data = SkData::MakeFromFileName(path);
    if (!data) {
        std::cerr << "Failed to load: " << path << std::endl;
        return nullptr;
    }
    return SkImages::DeferredFromEncodedData(data);
}

void run_benchmark() {
    auto bg_image = load_image("c:/hmp/GnollHack/win/win32/xpl/GnollHackX/GnollHackX/Assets/button_normal.png");
    auto border_image = load_image("c:/hmp/GnollHack/win/win32/xpl/GnollHackX/GnollHackX/Assets/button_normal.png");
    auto icon_image = load_image("c:/hmp/GnollHack/win/win32/xpl/GnollHackX/GnollHackX/Assets/button_normal.png");
    auto btn_image = load_image("c:/hmp/GnollHack/win/win32/xpl/GnollHackX/GnollHackX/Assets/button_normal.png");


    if (!bg_image || !border_image || !icon_image || !btn_image) {
        std::cerr << "Missing images!" << std::endl;
        // generate fallbacks just in case
        SkBitmap bitmap;
        bitmap.allocN32Pixels(100, 100);
        SkCanvas canvas(bitmap);
        canvas.clear(SK_ColorRED);
        icon_image = bitmap.asImage();
        bg_image = bitmap.asImage();
        border_image = bitmap.asImage();
        btn_image = bitmap.asImage();
    }

    SkImageInfo info = SkImageInfo::MakeN32Premul(CANVAS_WIDTH, CANVAS_height);
    auto surface = SkSurfaces::Raster(info);
    SkCanvas* canvas = surface->getCanvas();

    std::cout << "Starting benchmark with " << ITERATIONS << " iterations..." << std::endl;
    
    auto start = std::chrono::high_resolution_clock::now();

    for (int i = 0; i < ITERATIONS; i++) {
        // Clear canvas
        canvas->clear(SK_ColorBLACK);

        // 1. Draw Background (tiled)
        SkPaint bgPaint;
        int xTiles = (CANVAS_WIDTH / bg_image->width()) + 1;
        int yTiles = (CANVAS_height / bg_image->height()) + 1;
        for (int x = 0; x < xTiles; x++) {
            for (int y = 0; y < yTiles; y++) {
                canvas->drawImage(bg_image, x * bg_image->width(), y * bg_image->height(), SkSamplingOptions(), &bgPaint);
            }
        }

        // 1b. Draw Border tiles (top and bottom, left and right)
        float bWidth = border_image->width();
        float bHeight = border_image->height();
        for (int x = 0; x < (CANVAS_WIDTH / bWidth) + 1; x++) {
            canvas->drawImage(border_image, x * bWidth, 0, SkSamplingOptions(), &bgPaint);
            canvas->drawImage(border_image, x * bWidth, CANVAS_height - bHeight, SkSamplingOptions(), &bgPaint);
        }
        for (int y = 0; y < (CANVAS_height / bHeight) + 1; y++) {
            canvas->drawImage(border_image, 0, y * bHeight, SkSamplingOptions(), &bgPaint);
            canvas->drawImage(border_image, CANVAS_WIDTH - bWidth, y * bHeight, SkSamplingOptions(), &bgPaint);
        }

        // 2. Draw Image Carousel (large image in middle)
        SkRect carouselRect = SkRect::MakeXYWH(100, 200, CANVAS_WIDTH - 200, 400);
        canvas->drawImageRect(icon_image, carouselRect, SkSamplingOptions(), &bgPaint);

        // Draw multiple labels around (like CustomLabels)
        SkFont font;
        font.setSize(19.0f);
        SkPaint textPaint;
        textPaint.setAntiAlias(true);
        textPaint.setColor(SK_ColorWHITE);
        
        for (int k = 0; k < 6; k++) {
            canvas->drawSimpleText("Some Label Text", 15, SkTextEncoding::kUTF8, 150, 500 + k*30, font, textPaint);
        }

        // 3. Draw RowImageButtons inside a simulated ScrollView
        SkPaint rowPaint;
        for (int j = 0; j < NUM_ICONS; j++) {
            SkRect btnRect = SkRect::MakeXYWH(50, 650 + j * 120, CANVAS_WIDTH - 100, 100);
            // simulated 9-patch or stretch
            canvas->drawImageRect(btn_image, btnRect, SkSamplingOptions(), &rowPaint);

            // Draw image inside button
            SkRect iconDst = SkRect::MakeXYWH(70, 660 + j * 120, 80, 80);
            canvas->drawImageRect(icon_image, iconDst, SkSamplingOptions(), &rowPaint);
            
            // Draw text inside button
            canvas->drawSimpleText("Button Text", 11, SkTextEncoding::kUTF8, 170, 715 + j * 120, font, textPaint);
        }

        // 4. Draw modal background if any, and bottom close button
        SkRect closeBtnRect = SkRect::MakeXYWH(200, CANVAS_height - 150, CANVAS_WIDTH - 400, 80);
        canvas->drawImageRect(btn_image, closeBtnRect, SkSamplingOptions(), &rowPaint);
        canvas->drawSimpleText("Close Modal", 11, SkTextEncoding::kUTF8, 280, CANVAS_height - 100, font, textPaint);
    }

    auto end = std::chrono::high_resolution_clock::now();
    std::chrono::duration<double, std::milli> elapsed = end - start;
    
    std::cout << "Time taken: " << elapsed.count() << " ms" << std::endl;
    std::cout << "Average frame time: " << (elapsed.count() / ITERATIONS) << " ms" << std::endl;
}

int main() {
    run_benchmark();
    return 0;
}
