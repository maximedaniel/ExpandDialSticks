import numpy as np
import cv2



src_img = cv2.imread('calibration.jpg')
img = src_img
# Median Blur
#img = cv2.medianBlur(img, 3)
# Gaussian Blur
#img = cv2.GaussianBlur(img, (3,3), 0)

# Erosion
kernel = np.ones((5,5), np.uint8)
img = cv2.erode(src_img, kernel, iterations=1)

# kernel & convolution
kernel = np.array([[-1,-1,-1], [-1,9,-1], [-1,-1,-1]])
img = cv2.filter2D(img, -1, kernel)


gray = cv2.cvtColor(img,cv2.COLOR_BGR2GRAY)
gray = np.float32(gray)
dst = cv2.cornerHarris(gray,2,3,0.04)
#result is dilated for marking the corners, not important
dst = cv2.dilate(dst,None)
# Threshold for an optimal value, it may vary depending on the image.
img[dst>0.05*dst.max()]=[0,0,255]

# CHESSBOARD TRACKING

# termination criteria
criteria = (cv2.TERM_CRITERIA_EPS + cv2.TERM_CRITERIA_MAX_ITER, 30, 0.001)

# prepare object points, like (0,0,0), (1,0,0), (2,0,0) ....,(6,5,0)
objp = np.zeros((6*8,3), np.float32)
objp[:,:2] = np.mgrid[0:8,0:6].T.reshape(-1,2)

# Arrays to store object points and image points from all the images.
objpoints = [] # 3d point in real world space
imgpoints = [] # 2d points in image plane.

# Find the chess board corners
ret, corners = cv2.findChessboardCorners(img, (8,6),None)
# If found, add object points, image points (after refining them)
if ret == True:
    print('chess found !')
    objpoints.append(objp)
    corners2 = cv2.cornerSubPix(gray,corners,(11,11),(-1,-1),criteria)
    imgpoints.append(corners2)
    # Draw and display the corners
    img = cv2.drawChessboardCorners(img, (8,6), corners2,ret)


cv2.imshow('dst', np.hstack([src_img, img])) #np.hstack([img,sharpen, dst]))

if cv2.waitKey(0) & 0xff == 27:
    cv2.destroyAllWindows()