# UnityEditorTools
整理一些常用的工具，以及一些简单的实例工程，不定期更新    


##################################################################################
#2017-03-13 更新 
## 热更Assetbundle资源包 实例

-简单的热更加载流程整理，Assets\Examples\TableDataPack\AbHotUpdate.unity    PC & 阿里云搭建FTP测试通过    
-菜单栏[MenuItem("Helper/Clean Cache")]    
	协助测试用，清理本地缓存

##################################################################################
#2017-03-13 更新    
## AssetBundle打包 整理    

-Unity版本更新到Unity 5.5.0f3 (64-bit)，BuildAssetbundles(string outpath)弃用，打包工具做相应调整
 

##################################################################################
#2017-03-11 更新
## AssetBundle打包 整理       

-菜单栏[MenuItem("Helper/Table Window")]    
	Excel表格多了一步跟对应CS绑定输出ScriptObject    

-Assets\StreamingAssets 目录为各个不同标签的AB包整包输出目录，不同平台均共享这个目录，输出前会清空这个目录    
	这里是为了方便Examples演示用，所以不同平台都放在一个目录，实际开发中可以分开存放，可以避免反复切换平台需要反复打包    
	不过出包的时候需要把输出的AB包贴到游戏调用的统一目录（比如StreamingAssets）    

-内附加载表格实例（Assets\Examples\TableDataPack\TableDataPack.unity）PC，Android真机测试通过

-菜单栏[MenuItem("Helper/UITexturePack")]    
	Assets/UITexture/目录下面所有贴图资源打包    
   

-菜单栏[MenuItem("Helper/MD5Tools Window")]    
	给指定路径(示例中为StreamingAssets)下的资源生成MD5文件，用来做热更比对用    



##################################################################################
#2017-03-10 更新
## FTP文件操作整理 （未完成）

-这个工具是 Assetbundle打包热更 工作流的一部分，可以开放在打包工具界面上，方便开发人员及时热更资源包    
-内附读取指定FTP根目录文件列表实例（Assets\Examples\Ftp\FtpUpDownExample.unity）PC & 阿里云搭建FTP测试通过


##################################################################################
#2017-03-10 更新
## Excel表格打包(已更新)