<?php
return array(
	//'配置项'=>'配置值
    'URL_MODEL'          => '2', //URL模式
    'DB_TYPE'               =>  'mysql',     // 数据库类型
    'DB_HOST'               =>  'localhost', // 服务器地址
    'DB_NAME'               =>  'php',          // 数据库名
    'DB_USER'               =>  'root',      // 用户名
    'DB_PWD'                =>  '123456',          // 密码
    'DB_PORT'               =>  '3306',        // 端口
    'DB_PREFIX'             =>  '',    // 数据库表前缀
    'DB_PARAMS'          	=>  array(), // 数据库连接参数
    'DB_DEBUG'  			=>  TRUE, // 数据库调试模式 开启后可以记录SQL日志
    'DB_FIELDS_CACHE'       =>  true,        // 启用字段缓存
    'DB_CHARSET'            =>  'utf8',      // 数据库编码默认采用utf8
    'DB_DEPLOY_TYPE'        =>  0, // 数据库部署方式:0 集中式(单一服务器),1 分布式(主从服务器)
    'DB_RW_SEPARATE'        =>  false,       // 数据库读写是否分离 主从式有效
    'DB_MASTER_NUM'         =>  1, // 读写分离后 主服务器数量
    'DB_SLAVE_NO'           =>  '', // 指定从服务器序号

    'DB_CONFIG_PHP' => 'mysql://root:123456@localhost:3306/php#utf8',



    //启用模板页
    'LAYOUT_ON'=>true,
    'LAYOUT_NAME'=>'layout',

    //资源文件路径
    'TMPL_PARSE_STRING'=>array(
        '__IMG__'=>__ROOT__.'/Public/images',
        '__CSS__'=>__ROOT__.'/Public/css',
        '__JS__'=>__ROOT__.'/Public/js',
        '__Uploads__'=>__ROOT__.'/Public/uploads',
        '__DOC__'=>__ROOT__.'/Public/doc'
    ),

    //HOST主机地址
    'BASE_API'=>'http://127.0.0.1:9999/api',
);