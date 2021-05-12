import ffmpeg
import cv2
import numpy as np
import time

in_filename = "test0.mp4"
out_filename = "test0_bis.mp4"
width = 640
height = 480

# ffmpeg -f gdigrab -framerate 30 -i title="Calculatrice" test.mp4
cmd = ffmpeg.input('title=Unity3dMQTT', f="gdigrab", framerate=10).output('pipe:', format='rawvideo', pix_fmt='rgb24')
print(cmd.get_args())
process1 = (
    cmd
    .run_async(pipe_stdout=True, pipe_stderr=True)
)

while True:
    in_bytes = process1.stdout.read(width * height * 3)
    if not in_bytes:
        break
    in_frame = (
        np
        .frombuffer(in_bytes, np.uint8)
        .reshape([height, width, 3])
    )
    cv2.imshow('frame',in_frame)
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break
    time.sleep(0.1)

process1.kill()