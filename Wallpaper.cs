using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    public class VideoWallpaperModule : Module
    {
        private Texture2D[] _videoFrames;
        private int _currentFrame = 0;
        private int _framesSinceLastUpdate = 0;
        private int _targetFPS = 15;
        private bool _isPlaying = true;
        private bool _updatesEnabled = true; // 新增：控制更新是否启用
        private string _videoName;

        // 硬编码测试路径
        private string _testVideoPath = @"WallpaperVideos\test1";

        // 配置参数
        private float _opacity = 1.0f;
        private bool _pauseWhenFocused = false;

        // 添加静态实例用于全局访问
        private static VideoWallpaperModule _instance;
        public static VideoWallpaperModule Instance => _instance;

        public VideoWallpaperModule(Rectangle location, OS operatingSystem)
            : base(location, operatingSystem)
        {
            name = "VideoWallpaper";
            _videoName = "test1";
            _instance = this;
            _updatesEnabled = true; // 默认启用更新

            // 输出调试信息
            os.write("=== Video Wallpaper Debug Info ===");
            os.write("Test path: " + _testVideoPath);

            // 检查目录结构
            CheckDirectoryStructure();

            // 加载视频帧
            LoadTestVideoFrames();

            // 设置为全屏（超出屏幕边界）
            bounds = new Rectangle(-100, -100,
                os.ScreenManager.GraphicsDevice.Viewport.Width + 200,
                os.ScreenManager.GraphicsDevice.Viewport.Height + 200);

            os.write("VideoWallpaperModule constructor completed");
        }

        // 添加手动更新方法
        public void ManualUpdate(float t)
        {
            if (_updatesEnabled) // 只在启用时更新
            {
                Update(t);
            }
        }

        // 新增：停止更新
        public void StopUpdates()
        {
            _updatesEnabled = false;
            _isPlaying = false;
            os.write("Video wallpaper updates stopped");
        }

        // 新增：清理资源
        public void Cleanup()
        {
            StopUpdates();

            // 清理纹理资源
            if (_videoFrames != null)
            {
                foreach (var texture in _videoFrames)
                {
                    texture?.Dispose();
                }
                _videoFrames = null;
            }

            // 清除静态实例
            if (_instance == this)
            {
                _instance = null;
            }

            os.write("Video wallpaper resources cleaned up");
        }

        private void CheckDirectoryStructure()
        {
            try
            {
                // 检查根目录
                string rootPath = Directory.GetCurrentDirectory();
                os.write("Current directory: " + rootPath);

                // 检查测试路径是否存在
                string fullTestPath = Path.Combine(rootPath, _testVideoPath);
                os.write("Full test path: " + fullTestPath);

                if (Directory.Exists(fullTestPath))
                {
                    os.write("Test directory exists");

                    // 列出所有文件
                    string[] allFiles = Directory.GetFiles(fullTestPath, "*.*", SearchOption.AllDirectories);
                    os.write($"Found {allFiles.Length} files in directory:");

                    foreach (string file in allFiles)
                    {
                        FileInfo fi = new FileInfo(file);
                        os.write($"  - {fi.Name} ({fi.Length} bytes)");
                    }

                    // 特别检查PNG文件
                    string[] pngFiles = Directory.GetFiles(fullTestPath, "*.png");
                    os.write($"Found {pngFiles.Length} PNG files:");

                    foreach (string pngFile in pngFiles)
                    {
                        FileInfo fi = new FileInfo(pngFile);
                        os.write($"  - {fi.Name}");
                    }
                }
                else
                {
                    os.write("Test directory does not exist");
                    os.write("Creating directory structure for testing...");

                    // 创建测试目录
                    Directory.CreateDirectory(fullTestPath);
                    os.write("Created directory: " + fullTestPath);

                    // 创建一些测试文件说明
                    string readmePath = Path.Combine(fullTestPath, "README.txt");
                    File.WriteAllText(readmePath, "Place your PNG sequence frames here:\nframe_0001.png\nframe_0002.png\n...etc");
                    os.write("Created README file");
                }
            }
            catch (Exception ex)
            {
                os.write("ERROR checking directory: " + ex.Message);
            }
        }

        private void LoadTestVideoFrames()
        {
            try
            {
                string fullTestPath = Path.Combine(Directory.GetCurrentDirectory(), _testVideoPath);

                if (!Directory.Exists(fullTestPath))
                {
                    os.write("ERROR: Test directory still doesn't exist after creation attempt");
                    return;
                }

                // 查找PNG序列帧
                string[] frameFiles = Directory.GetFiles(fullTestPath, "*.png");

                if (frameFiles.Length == 0)
                {
                    os.write("No PNG files found. Looking for any image files...");
                    frameFiles = Directory.GetFiles(fullTestPath, "*.*");
                    frameFiles = Array.FindAll(frameFiles, file =>
                        file.ToLower().EndsWith(".png") ||
                        file.ToLower().EndsWith(".jpg") ||
                        file.ToLower().EndsWith(".jpeg"));
                }

                if (frameFiles.Length == 0)
                {
                    os.write("No image files found in test directory");
                    os.write("Please add PNG sequence frames to: " + fullTestPath);
                    return;
                }

                // 排序文件（确保正确的播放顺序）
                Array.Sort(frameFiles);

                os.write($"Loading {frameFiles.Length} frames...");

                _videoFrames = new Texture2D[frameFiles.Length];
                int loadedCount = 0;

                for (int i = 0; i < frameFiles.Length; i++)
                {
                    try
                    {
                        using (var stream = File.OpenRead(frameFiles[i]))
                        {
                            _videoFrames[i] = Texture2D.FromStream(
                                os.ScreenManager.GraphicsDevice,
                                stream
                            );
                            loadedCount++;

                            // 每加载10帧输出一次进度
                            if (loadedCount % 10 == 0)
                            {
                                os.write($"Loaded {loadedCount}/{frameFiles.Length} frames...");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        os.write($"ERROR loading frame {Path.GetFileName(frameFiles[i])}: {ex.Message}");
                    }
                }

                os.write($"Successfully loaded {loadedCount} video frames");
                _isPlaying = true;

            }
            catch (Exception ex)
            {
                os.write("ERROR loading test video frames: " + ex.Message);
                _isPlaying = false;
            }
        }

        public override void Update(float t)
        {
            base.Update(t);

            // 如果更新被禁用，立即返回
            if (!_updatesEnabled)
                return;

            if (!_isPlaying || _videoFrames == null || _videoFrames.Length == 0)
            {
                return;
            }

            // 游戏获得焦点时暂停（可选）
            if (_pauseWhenFocused && os.IsActive)
                return;

            _framesSinceLastUpdate++;

            // 根据目标FPS更新帧
            int updateInterval = Math.Max(1, 60 / _targetFPS);
            if (_framesSinceLastUpdate >= updateInterval)
            {
                int oldFrame = _currentFrame;
                _currentFrame = (_currentFrame + 1) % _videoFrames.Length;
                _framesSinceLastUpdate = 0;
            }
        }

        public override void Draw(float t)
        {
            // 如果更新被禁用，不绘制任何内容
            if (!_updatesEnabled)
                return;

            // 不调用 base.Draw() 避免绘制模块边框

            if (!_isPlaying || _videoFrames == null || _currentFrame >= _videoFrames.Length)
            {
                return;
            }

            // 绘制视频帧（全屏）
            Color tint = Color.White * _opacity;
            spriteBatch.Draw(_videoFrames[_currentFrame], bounds, tint);
        }

        public void SetOpacity(float opacity)
        {
            _opacity = MathHelper.Clamp(opacity, 0f, 1f);
            os.write($"Opacity set to: {_opacity}");
        }

        public void SetFPS(int fps)
        {
            _targetFPS = (int)MathHelper.Clamp(fps, 1, 60);
            os.write($"FPS set to: {_targetFPS}");
        }

        public void Play()
        {
            _isPlaying = true;
            os.write("Video playback started");
        }

        public void Pause()
        {
            _isPlaying = false;
            os.write("Video playback paused");
        }

        public float GetOpacity()
        {
            return _opacity;
        }

        public void Stop()
        {
            _isPlaying = false;
            _currentFrame = 0;
            os.write("Video playback stopped");
        }

        public override void ()
        {
            // 确保模块被杀死时清理资源
            Cleanup();
            base.Killed();
        }
    }
}
