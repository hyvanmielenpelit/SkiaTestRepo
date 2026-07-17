#include "include/core/SkBitmap.h"
#include "include/core/SkCanvas.h"
#include "include/core/SkImage.h"
#include "include/core/SkPaint.h"
#include "include/core/SkRect.h"
#include "include/core/SkSurface.h"
#include "include/core/SkData.h"
#include "include/core/SkMilestone.h"
#include <chrono>
#include <iostream>
#include <fstream>
#include <vector>
#include <numeric>

#ifndef SKIA_GIT_HASH
#define SKIA_GIT_HASH "unknown"
#endif

const int CANVAS_WIDTH = 3200;
const int CANVAS_HEIGHT = 2000;
const int NUM_FRAMES = 180;

sk_sp<SkImage> load_image(const char* path) {
    auto data = SkData::MakeFromFileName(path);
    if (!data) {
        std::cerr << "Failed to load: " << path << std::endl;
        return nullptr;
    }
    auto image = SkImages::DeferredFromEncodedData(data);
    if (!image) {
        std::cerr << "Failed to decode image from data: " << path << std::endl;
    }
    return image;
}

int main() {
    std::cout << "Skia Milestone: m" << SK_MILESTONE << std::endl;
    std::cout << "Skia Commit: " << SKIA_GIT_HASH << std::endl;

    auto img1 = load_image("C:/repos/SkiaTestRepo/SkiaSharpTest/Resources/Raw/main-menu-gnoll.jpg");
    auto img2 = load_image("C:/repos/SkiaTestRepo/SkiaSharpTest/Resources/Raw/main-menu-dwarf.jpg");

    if (!img1 || !img2) {
        std::cerr << "Failed to load test images from SkiaSharpTest resources!" << std::endl;
        return 1;
    }

    SkImageInfo info = SkImageInfo::MakeN32Premul(CANVAS_WIDTH, CANVAS_HEIGHT);
    auto surface = SkSurfaces::Raster(info);
    if (!surface) {
        std::cerr << "Failed to create raster surface!" << std::endl;
        return 1;
    }
    SkCanvas* canvas = surface->getCanvas();

    std::cout << "Starting cross-fade benchmark (" << CANVAS_WIDTH << "x" << CANVAS_HEIGHT << ")..." << std::endl;

    std::vector<double> frame_times;
    SkRect dstRect = SkRect::MakeXYWH(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

    auto total_start = std::chrono::high_resolution_clock::now();

    for (int i = 0; i < NUM_FRAMES; ++i) {
        auto frame_start = std::chrono::high_resolution_clock::now();

        canvas->clear(SK_ColorBLACK);

        float alpha = i / (float)(NUM_FRAMES - 1);

        // Draw first image with (1 - alpha)
        SkPaint paint1;
        paint1.setAlphaf(1.0f - alpha);
        canvas->drawImageRect(img1, dstRect, SkSamplingOptions(SkFilterMode::kLinear), &paint1);

        // Draw second image with alpha
        SkPaint paint2;
        paint2.setAlphaf(alpha);
        canvas->drawImageRect(img2, dstRect, SkSamplingOptions(SkFilterMode::kLinear), &paint2);

        auto frame_end = std::chrono::high_resolution_clock::now();
        std::chrono::duration<double, std::milli> frame_elapsed = frame_end - frame_start;
        frame_times.push_back(frame_elapsed.count());
    }

    auto total_end = std::chrono::high_resolution_clock::now();
    std::chrono::duration<double, std::milli> total_elapsed = total_end - total_start;

    double total_ms = total_elapsed.count();
    double avg_ms = total_ms / NUM_FRAMES;
    double min_ms = frame_times[0];
    double max_ms = frame_times[0];
    double sum_ms = 0;
    for (double t : frame_times) {
        if (t < min_ms) min_ms = t;
        if (t > max_ms) max_ms = t;
        sum_ms += t;
    }

    std::cout << "Benchmark finished." << std::endl;
    std::cout << "Total time: " << total_ms << " ms" << std::endl;
    std::cout << "Average frame time: " << avg_ms << " ms" << std::endl;

    // Write to file
    std::ofstream out("benchmark_results.txt");
    if (out.is_open()) {
        out << "Skia Milestone: m" << SK_MILESTONE << "\n";
        out << "Skia Commit: " << SKIA_GIT_HASH << "\n";
        out << "Canvas Size: " << CANVAS_WIDTH << "x" << CANVAS_HEIGHT << "\n";
        out << "Total Frames: " << NUM_FRAMES << "\n";
        out << "Total Time: " << total_ms << " ms\n";
        out << "Average Frame Time: " << avg_ms << " ms\n";
        out << "Min Frame Time: " << min_ms << " ms\n";
        out << "Max Frame Time: " << max_ms << " ms\n\n";
        out << "Frame,Time (ms)\n";
        for (size_t i = 0; i < frame_times.size(); ++i) {
            out << i << "," << frame_times[i] << "\n";
        }
        out.close();
        std::cout << "Results written to benchmark_results.txt" << std::endl;
    } else {
        std::cerr << "Failed to write results to benchmark_results.txt" << std::endl;
    }

    return 0;
}
