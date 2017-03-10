# UnityEditorTools
整理一些常用的工具，不定期更新


##################################################################################
2017-03-10 更新
# Excel表格打包
-菜单栏[MenuItem("Helper/Table Window")]    
        Unity5.x 每个资源都会有一个AssetbundleName 该属性相同的资源会被打包到同一个bundle    
-Assets\StreamingAssets\table 目录为整包输出目录，不同平台均共享这个目录，输出前会清空这个目录    
-内附加载实例（Assets\Examples\TableDataPack\TableDataPack.unity）PC，Android真机测试通过



##################################################################################
2017-03-10 更新
# FTP文件操作整理 （未完成）

-这个工具是 Assetbundle打包热更 工作流的一部分，可以开放在打包工具界面上，方便开发人员及时热更资源包    
-内附读取指定FTP根目录文件列表实例（Assets\Examples\Ftp\FtpUpDownExample.unity）PC & 阿里云搭建FTP测试通过