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
#include <string>

#ifndef SKIA_GIT_HASH
#define SKIA_GIT_HASH "unknown"
#endif

const int CANVAS_WIDTH = 3200;
const int CANVAS_HEIGHT = 2000;
const int CROSSFADE_FRAMES = 180;
const int SPEED_TEST_FRAMES = 200;

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
    SkRect dstRect = SkRect::MakeXYWH(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

    // ==========================================
    // TEST 1: Cross-Fade Test
    // ==========================================
    std::cout << "\nStarting Test 1: Cross-Fade Test (" << CROSSFADE_FRAMES << " frames)..." << std::endl;
    std::vector<double> crossfade_times;
    auto t1_start = std::chrono::high_resolution_clock::now();

    for (int i = 0; i < CROSSFADE_FRAMES; ++i) {
        auto frame_start = std::chrono::high_resolution_clock::now();

        canvas->clear(SK_ColorBLACK);

        float alpha = i / (float)(CROSSFADE_FRAMES - 1);

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
        crossfade_times.push_back(frame_elapsed.count());
    }
    auto t1_end = std::chrono::high_resolution_clock::now();
    std::chrono::duration<double, std::milli> t1_elapsed = t1_end - t1_start;

    double t1_total_ms = t1_elapsed.count();
    double t1_avg_ms = t1_total_ms / CROSSFADE_FRAMES;
    double t1_min_ms = crossfade_times[0];
    double t1_max_ms = crossfade_times[0];
    for (double t : crossfade_times) {
        if (t < t1_min_ms) t1_min_ms = t;
        if (t > t1_max_ms) t1_max_ms = t;
    }

    std::cout << "Test 1 Finished. Total time: " << t1_total_ms << " ms, Avg: " << t1_avg_ms << " ms" << std::endl;

    // ==========================================
    // TEST 2: Speed Test
    // ==========================================
    std::cout << "\nStarting Test 2: Speed Test (" << SPEED_TEST_FRAMES << " frames)..." << std::endl;
    std::vector<double> speed_times;
    auto t2_start = std::chrono::high_resolution_clock::now();

    for (int i = 0; i < SPEED_TEST_FRAMES; ++i) {
        auto frame_start = std::chrono::high_resolution_clock::now();

        canvas->clear(SK_ColorBLACK);

        // Draw alternating images at 100% opacity
        sk_sp<SkImage> currentImg = (i % 2 == 0) ? img1 : img2;
        canvas->drawImageRect(currentImg, dstRect, SkSamplingOptions(SkFilterMode::kLinear), nullptr);

        auto frame_end = std::chrono::high_resolution_clock::now();
        std::chrono::duration<double, std::milli> frame_elapsed = frame_end - frame_start;
        speed_times.push_back(frame_elapsed.count());
    }
    auto t2_end = std::chrono::high_resolution_clock::now();
    std::chrono::duration<double, std::milli> t2_elapsed = t2_end - t2_start;

    double t2_total_ms = t2_elapsed.count();
    double t2_avg_ms = t2_total_ms / SPEED_TEST_FRAMES;
    double t2_min_ms = speed_times[0];
    double t2_max_ms = speed_times[0];
    for (double t : speed_times) {
        if (t < t2_min_ms) t2_min_ms = t;
        if (t > t2_max_ms) t2_max_ms = t;
    }

    std::cout << "Test 2 Finished. Total time: " << t2_total_ms << " ms, Avg: " << t2_avg_ms << " ms" << std::endl;

    // ==========================================
    // Write results to file
    // ==========================================
    std::string filename = "benchmark_results_m" + std::to_string(SK_MILESTONE) + ".txt";
    std::ofstream out(filename);
    if (out.is_open()) {
        out << "Skia Milestone: m" << SK_MILESTONE << "\n";
        out << "Skia Commit: " << SKIA_GIT_HASH << "\n";
        out << "Canvas Size: " << CANVAS_WIDTH << "x" << CANVAS_HEIGHT << "\n\n";

        out << "=== TEST 1: Cross-Fade Test ===\n";
        out << "Total Frames: " << CROSSFADE_FRAMES << "\n";
        out << "Total Time: " << t1_total_ms << " ms\n";
        out << "Average Frame Time: " << t1_avg_ms << " ms\n";
        out << "Min Frame Time: " << t1_min_ms << " ms\n";
        out << "Max Frame Time: " << t1_max_ms << " ms\n\n";

        out << "=== TEST 2: Speed Test ===\n";
        out << "Total Frames: " << SPEED_TEST_FRAMES << "\n";
        out << "Total Time: " << t2_total_ms << " ms\n";
        out << "Average Frame Time: " << t2_avg_ms << " ms\n";
        out << "Min Frame Time: " << t2_min_ms << " ms\n";
        out << "Max Frame Time: " << t2_max_ms << " ms\n\n";

        out << "=== Detailed Frame Data ===\n";
        out << "Frame,Test 1 Time (ms),Test 2 Time (ms)\n";
        int max_frames = std::max(CROSSFADE_FRAMES, SPEED_TEST_FRAMES);
        for (int i = 0; i < max_frames; ++i) {
            out << i << ",";
            if (i < CROSSFADE_FRAMES) out << crossfade_times[i];
            out << ",";
            if (i < SPEED_TEST_FRAMES) out << speed_times[i];
            out << "\n";
        }
        out.close();
        std::cout << "\nResults written to " << filename << std::endl;
    } else {
        std::cerr << "\nFailed to write results to " << filename << std::endl;
    }

    return 0;
}
