# 第四章 系统实现

## 4.1 系统运行界面

### 4.1.1 登录与注册页面

身份验证与注册新用户页面，验证成功将根据用户权限等级进入不同主页，新用户默认权限为用户等级。

![0](https://gitee.com/wonaren/picgo/raw/master/picture/clip_image002.png)![img](https://gitee.com/wonaren/picgo/raw/master/picture/clip_image004.png)

### 4.1.2主要功能界面

**航班搜索页面**：该页面可根据“出发城市、到达城市、出发时间”来对未来的航班进行模糊查询，查询项全为空即刷新。点击列表中的元素可以在下方显示航班详细信息，点击“查看动态”可以根据表格选择的航班信息进入航班动态页面，直观查看航班信息与动态；点击“去购买”按钮会根据选择的航班和用户信息进入购买机票页面。

![img](https://gitee.com/wonaren/picgo/raw/master/picture/clip_image006.png)

**航班动态页面：**根据“查看动态”带入的信息或者输入想要查询的航班号即可查询航班的动态，相关信息将会根据当前时间在面板上渲染最新的数据，其中进度条表示航班飞行进度，左边时间盘显示当前航班起飞时间。

![11](https://gitee.com/wonaren/picgo/raw/master/picture/11.png)

![img](https://gitee.com/wonaren/picgo/raw/master/picture/wps1.jpg)

![img](https://gitee.com/wonaren/picgo/raw/master/picture/wps2.jpg)

**机票预约与订单：**在此页面用户可以根据需求自己的需求填写预约订单，可以是存在的航班也可以是不存在的航班（管理员可以查看到所有预约订单号，不存在的航班可以通过管理员来添加），通知人为乘客，如果用户没有添加不必担心，在点击“立即预约”按钮通过逻辑判断后可以根据用户是否有该乘客信息来自动添加该乘客信息。预约成功后将在个人中心页面收到“预约成功”消息通知并更新数据表。

![20预约](https://gitee.com/wonaren/picgo/raw/master/picture/clip_image014.png)

![21预约填写](https://gitee.com/wonaren/picgo/raw/master/picture/clip_image016.png)

**个人中心：**

①乘客列表页面，用于乘客信息的管理，用户可以根据自己的需要自行添加、修改（证件号码不可更改）或删除乘客信息。“清空”按钮用于一键清空当前已经填写的文本框，方便用户添加乘客。

![img](https://gitee.com/wonaren/picgo/raw/master/picture/clip_image018.png)

②消息页面：用于显示接受到的所有通知类消息，选中列表项（消息标题+消息ID）即可查看消息详情，如果是“预约处理成功”的消息，消息框下方将出现“去买票”按钮，点击可以根据消息框的信息切片提取关键信息作为参数打开机票购买页面，实现客户需求，每则消息下方均有消息发送的时间。

![img](https://gitee.com/wonaren/picgo/raw/master/picture/clip_image020.png)

![img](https://gitee.com/wonaren/picgo/raw/master/picture/clip_image022.png)

③其他页面：用户可在此页面修改登录用户密码。

![img](https://gitee.com/wonaren/picgo/raw/master/picture/wps3.jpg)

**管理员模式：**此页面仅管理员可见，所有标签页切换时会触发对应或公有的数据更新函数，用于渲染最新数据。

①航班管理页面：管理员在此页面可以对航班进行增删改查操作，字段前面的复选框表示，是否包含该字段进行查询。修改航班可以直接在表格显示框里修改，修改完成后点击“修改航班”按钮可以完成修改操作，并根据修改的合理性更新数据（例如：修改航班出发时间为1月12日，修改航班状态为2（1-航班计划，2-航班在飞，3-航班结束），当前时间为1月11日，不符合现实逻辑，系统将自动将航班状态改为1）；点击选中航班，点击“删除航班”通过逻辑判断后可以删除航班，所有操作完成后均会刷新数据。

![img](https://gitee.com/wonaren/picgo/raw/master/picture/clip_image026.png)

②预约订单管理页面：在此页面，结构简单管理员可以点击“一键处理”按钮处理所有状态为“进行中”的订单，并显示订单处理详情（未处理订单提示信息会根据匹配结果显示对应原因），成功处理或未处理订单数，如果所有订单都是“已完成”将显示“所有订单已经处理完成!”。

![img](https://gitee.com/wonaren/picgo/raw/master/picture/clip_image028.png)

## 4.2 项目文件截图

![img](https://gitee.com/wonaren/picgo/raw/master/picture/clip_image029.png)

**程序窗口：**Form1为登录窗口，MainForm为主程序窗口，PayForm为机票购买窗口，SeatSelect为座位选择窗口

**资源文件：**Resources为程序用到的图片资源文件

**相关类：**DBcon为数据库连接类
