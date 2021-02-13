# image_tracking (In Progess)

image_tracking with distorted images by using head pose(yaw, roll and pitch). <br>
![](./images/img.png)

#### yaw, roll, pitch

<a href="https://www.researchgate.net/figure/Orientation-of-the-head-in-terms-of-pitch-roll-and-yaw-movements-describing-the-three_fig1_279291928"><img src="https://www.researchgate.net/profile/Tsang_Ing_Ren/publication/279291928/figure/fig1/AS:292533185462272@1446756754388/Orientation-of-the-head-in-terms-of-pitch-roll-and-yaw-movements-describing-the-three.png" alt="Orientation of the head in terms of pitch , roll , and yaw movements describing the three degrees of freedom of a human head."/></a>

#### base_image_size
+ width, height = original_image.shape
+ base_width_start = width // 4
+ base_width_end = width // 4 * 3
+ base_height_start = height // 4
+ base_height_ebd = height // 4 * 3

#### inner_base_image_size
+ width, height = base_image.shape
+ inner_base_width_start = width // 4
+ inner_base_width_end = width // 4 * 3
+ inner_base_height_start = height // 4
+ inner_base_height_ebd = height // 4 * 3

### Todo
- [x] [2d](https://github.com/sammiee5311/3_dimension_snake_game/tree/main/2d_practice)
- [ ] 3 dimention snake game

### reference

+ [Sharp Accent](https://www.youtube.com/channel/UCq9_1E5HE4c_xmhzD3r7VMw)
