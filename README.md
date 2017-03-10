# UnityEditorTools
整理一些常用的工具，不定期更新


##################################################################################
2017-03-10 更新
Unity (基于5.x) 常用工具整理（一）：Excel表格打包
#菜单栏[MenuItem("Helper/Table Window")]
	Unity5.x 每个资源都会有一个AssetbundleName 该属性相同的资源会被打包到同一个bundle
#Assets\StreamingAssets\table 目录为整包输出目录，不同平台均共享这个目录，输出前会清空这个目录
#内附加载实例（Assets\Examples\TableDataPack\TableDataPack.unity）PC，Android真机测试通过