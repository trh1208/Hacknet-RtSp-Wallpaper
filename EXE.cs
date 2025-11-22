using System;
using System.IO;
using Hacknet.Gui;
using Microsoft.Xna.Framework;

namespace Hacknet
{
    public class VideoWallpaperExe : Pathfinder.Executable.BaseExecutable
    {
        private VideoWallpaperModule _videoModule;
        private float _opacity = 1.0f;
        private int _fps = 15;
        private bool _moduleAdded = false;
        private bool _isExiting = false;

        public VideoWallpaperExe(Rectangle location, OS operatingSystem, string[] args)
        : base(location, operatingSystem, args)
        {
            name = "VideoWallpaper";
            ramCost = 220;
            IdentifierName = "Video Wallpaper";

            if (args.Length > 1)
                float.TryParse(args[1], out _opacity);

            if (args.Length > 2)
                int.TryParse(args[2], out _fps);
        }

        public override void LoadContent()
        {
            base.LoadContent();

            os.write("=== Video Wallpaper Test ===");
            os.write("Using hardcoded test path: WallpaperVideos\\test1");
            os.write("Parameters: opacity=" + _opacity + ", fps=" + _fps);

            // 清理之前的实例
            if (VideoWallpaperModule.Instance != null)
            {
                os.modules.Remove(VideoWallpaperModule.Instance);
                VideoWallpaperModule.Instance?.Cleanup();
            }

            // 创建全屏视频模块
            _videoModule = new VideoWallpaperModule(
                new Rectangle(0, 0,
                    os.ScreenManager.GraphicsDevice.Viewport.Width,
                    os.ScreenManager.GraphicsDevice.Viewport.Height),
                os
            );

            _videoModule.SetOpacity(_opacity);
            _videoModule.SetFPS(_fps);

            // 添加到OS模块列表（作为背景）
            os.modules.Insert(0, _videoModule);
            _moduleAdded = true;

            os.write("Video wallpaper test initialized - Module added to os.modules");
        }

        public override void Update(float t)
        {
            base.Update(t);

            // EXE窗口关闭时清理视频模块
            if ((isExiting || _isExiting) && _videoModule != null && _moduleAdded)
            {
                os.write("Cleaning up video wallpaper module...");

                // 停止模块更新
                _videoModule.StopUpdates();

                // 从模块列表中移除
                os.modules.Remove(_videoModule);

                // 清理资源
                _videoModule.Cleanup();

                _moduleAdded = false;
                _videoModule = null;

                os.write("Video wallpaper stopped and cleaned up");
            }
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            drawOutline();

            // 显示控制界面
            Rectangle contentBounds = new Rectangle(
                bounds.X + 2,
                bounds.Y + PANEL_HEIGHT,
                bounds.Width - 4,
                bounds.Height - PANEL_HEIGHT - 2
            );

            spriteBatch.Draw(Utils.white, contentBounds, os.darkBackgroundColor);

            int buttonY = contentBounds.Y + 10;
            int buttonWidth = contentBounds.Width - 20;
            int buttonHeight = 25;
            int buttonSpacing = 5;

            // 标题
            TextItem.doFontLabel(new Vector2(contentBounds.X + 10, buttonY),
                "Video Wallpaper Test", GuiData.font, Color.White);
            buttonY += 30;

            if (_videoModule != null && !_isExiting)
            {
                // 控制按钮
                if (Button.doButton(1001, contentBounds.X + 10, buttonY, buttonWidth, buttonHeight, "Pause", os.lockedColor))
                {
                    _videoModule.Pause();
                }
                buttonY += buttonHeight + buttonSpacing;

                if (Button.doButton(1002, contentBounds.X + 10, buttonY, buttonWidth, buttonHeight, "Play", os.unlockedColor))
                {
                    _videoModule.Play();
                }
                buttonY += buttonHeight + buttonSpacing;

                if (Button.doButton(1003, contentBounds.X + 10, buttonY, buttonWidth, buttonHeight, "Opacity -", os.moduleColorSolid))
                {
                    _videoModule.SetOpacity(_videoModule.GetOpacity() - 0.1f);
                }
                buttonY += buttonHeight + buttonSpacing;

                if (Button.doButton(1004, contentBounds.X + 10, buttonY, buttonWidth, buttonHeight, "Opacity +", os.moduleColorSolid))
                {
                    _videoModule.SetOpacity(_videoModule.GetOpacity() + 0.1f);
                }
                buttonY += buttonHeight + buttonSpacing;

                if (Button.doButton(1005, contentBounds.X + 10, buttonY, buttonWidth, buttonHeight, "Stop & Exit", os.brightLockedColor))
                {
                    _isExiting = true;
                    isExiting = true;
                }
                buttonY += buttonHeight + 15;

                // 状态信息
                string status = "Status: Active";
                TextItem.doFontLabel(new Vector2(contentBounds.X + 10, buttonY), status, GuiData.smallfont, Color.LightGreen);
            }
            else
            {
                TextItem.doFontLabel(new Vector2(contentBounds.X + 10, buttonY),
                    "Video wallpaper not loaded", GuiData.smallfont, Color.Red);
            }
        }

        public override void Killed()
        {
            // 确保在程序被杀死时也清理资源
            if (_videoModule != null && _moduleAdded)
            {
                os.modules.Remove(_videoModule);
                _videoModule.Cleanup();
                _moduleAdded = false;
                _videoModule = null;
            }
            base.Killed();
        }
    }
}
