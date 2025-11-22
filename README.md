EXE：#VW#
首先创建Hacknet\WallpaperVideos\test1目录。
使用ffmpeg将你的MP4分割为多个png格式图片，要求文件名为frame_XXXX.png格式。
使用这个指令快速分割 .\ffmpeg.exe -i 1.mp4 -t 10 -r 30 "N/frame_%04d.png"（这将将1.mp4拆分到N目录下，请确保N目录存在。从1.mp4提取前10s内容，拆分为30张每次，共计300张）
在游戏中启动VW即可
