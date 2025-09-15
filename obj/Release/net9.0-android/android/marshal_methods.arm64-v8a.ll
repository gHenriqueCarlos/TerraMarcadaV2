; ModuleID = 'marshal_methods.arm64-v8a.ll'
source_filename = "marshal_methods.arm64-v8a.ll"
target datalayout = "e-m:e-i8:8:32-i16:16:32-i64:64-i128:128-n32:64-S128"
target triple = "aarch64-unknown-linux-android21"

%struct.MarshalMethodName = type {
	i64, ; uint64_t id
	ptr ; char* name
}

%struct.MarshalMethodsManagedClass = type {
	i32, ; uint32_t token
	ptr ; MonoClass klass
}

@assembly_image_cache = dso_local local_unnamed_addr global [155 x ptr] zeroinitializer, align 8

; Each entry maps hash of an assembly name to an index into the `assembly_image_cache` array
@assembly_image_cache_hashes = dso_local local_unnamed_addr constant [465 x i64] [
	i64 u0x0071cf2d27b7d61e, ; 0: lib_Xamarin.AndroidX.SwipeRefreshLayout.dll.so => 92
	i64 u0x02123411c4e01926, ; 1: lib_Xamarin.AndroidX.Navigation.Runtime.dll.so => 88
	i64 u0x022e81ea9c46e03a, ; 2: lib_CommunityToolkit.Maui.Core.dll.so => 37
	i64 u0x02abedc11addc1ed, ; 3: lib_Mono.Android.Runtime.dll.so => 153
	i64 u0x032267b2a94db371, ; 4: lib_Xamarin.AndroidX.AppCompat.dll.so => 66
	i64 u0x0363ac97a4cb84e6, ; 5: SQLitePCLRaw.provider.e_sqlite3.dll => 64
	i64 u0x043032f1d071fae0, ; 6: ru/Microsoft.Maui.Controls.resources => 24
	i64 u0x044440a55165631e, ; 7: lib-cs-Microsoft.Maui.Controls.resources.dll.so => 2
	i64 u0x046eb1581a80c6b0, ; 8: vi/Microsoft.Maui.Controls.resources => 30
	i64 u0x0517ef04e06e9f76, ; 9: System.Net.Primitives => 125
	i64 u0x0565d18c6da3de38, ; 10: Xamarin.AndroidX.RecyclerView => 90
	i64 u0x0581db89237110e9, ; 11: lib_System.Collections.dll.so => 109
	i64 u0x05989cb940b225a9, ; 12: Microsoft.Maui.dll => 51
	i64 u0x06076b5d2b581f08, ; 13: zh-HK/Microsoft.Maui.Controls.resources => 31
	i64 u0x06388ffe9f6c161a, ; 14: System.Xml.Linq.dll => 146
	i64 u0x0680a433c781bb3d, ; 15: Xamarin.AndroidX.Collection.Jvm => 74
	i64 u0x07c57877c7ba78ad, ; 16: ru/Microsoft.Maui.Controls.resources.dll => 24
	i64 u0x07dcdc7460a0c5e4, ; 17: System.Collections.NonGeneric => 107
	i64 u0x088b9ffb2d85b417, ; 18: lib_Maui.GoogleMaps.dll.so => 54
	i64 u0x08f3c9788ee2153c, ; 19: Xamarin.AndroidX.DrawerLayout => 79
	i64 u0x0919c28b89381a0b, ; 20: lib_Microsoft.Extensions.Options.dll.so => 47
	i64 u0x092266563089ae3e, ; 21: lib_System.Collections.NonGeneric.dll.so => 107
	i64 u0x098b50f911ccea8d, ; 22: lib_Xamarin.GooglePlayServices.Basement.dll.so => 99
	i64 u0x09d144a7e214d457, ; 23: System.Security.Cryptography => 138
	i64 u0x0a832f2c97e71637, ; 24: Xamarin.AndroidX.Camera.Video => 71
	i64 u0x0b3b632c3bbee20c, ; 25: sk/Microsoft.Maui.Controls.resources => 25
	i64 u0x0b6aff547b84fbe9, ; 26: Xamarin.KotlinX.Serialization.Core.Jvm => 103
	i64 u0x0be2e1f8ce4064ed, ; 27: Xamarin.AndroidX.ViewPager => 93
	i64 u0x0c3ca6cc978e2aae, ; 28: pt-BR/Microsoft.Maui.Controls.resources => 21
	i64 u0x0c3d7adcdb333bf0, ; 29: Xamarin.AndroidX.Camera.Lifecycle => 70
	i64 u0x0c3dd9438f54f672, ; 30: lib_Xamarin.GooglePlayServices.Maps.dll.so => 100
	i64 u0x0c59ad9fbbd43abe, ; 31: Mono.Android => 154
	i64 u0x0c7790f60165fc06, ; 32: lib_Microsoft.Maui.Essentials.dll.so => 52
	i64 u0x0e14e73a54dda68e, ; 33: lib_System.Net.NameResolution.dll.so => 124
	i64 u0x0ec01b05613190b9, ; 34: SkiaSharp.Views.Android.dll => 57
	i64 u0x102a31b45304b1da, ; 35: Xamarin.AndroidX.CustomView => 78
	i64 u0x10f6cfcbcf801616, ; 36: System.IO.Compression.Brotli => 117
	i64 u0x118d570f508803d1, ; 37: Xamarin.AndroidX.Camera.Camera2.dll => 68
	i64 u0x11d18c1961d68c44, ; 38: lib_TerraMarcadaV2.dll.so => 104
	i64 u0x125b7f94acb989db, ; 39: Xamarin.AndroidX.RecyclerView.dll => 90
	i64 u0x13a01de0cbc3f06c, ; 40: lib-fr-Microsoft.Maui.Controls.resources.dll.so => 8
	i64 u0x13f1e5e209e91af4, ; 41: lib_Java.Interop.dll.so => 152
	i64 u0x13f1e880c25d96d1, ; 42: he/Microsoft.Maui.Controls.resources => 9
	i64 u0x143d8ea60a6a4011, ; 43: Microsoft.Extensions.DependencyInjection.Abstractions => 44
	i64 u0x16110e92252778a1, ; 44: InTheHand.Net.Bluetooth => 40
	i64 u0x17125c9a85b4929f, ; 45: lib_netstandard.dll.so => 150
	i64 u0x17b56e25558a5d36, ; 46: lib-hu-Microsoft.Maui.Controls.resources.dll.so => 12
	i64 u0x17f9358913beb16a, ; 47: System.Text.Encodings.Web => 140
	i64 u0x18402a709e357f3b, ; 48: lib_Xamarin.KotlinX.Serialization.Core.Jvm.dll.so => 103
	i64 u0x18f0ce884e87d89a, ; 49: nb/Microsoft.Maui.Controls.resources.dll => 18
	i64 u0x1a040febb58bf51e, ; 50: lib_Xamarin.AndroidX.Camera.View.dll.so => 72
	i64 u0x1a91866a319e9259, ; 51: lib_System.Collections.Concurrent.dll.so => 105
	i64 u0x1aac34d1917ba5d3, ; 52: lib_System.dll.so => 149
	i64 u0x1aad60783ffa3e5b, ; 53: lib-th-Microsoft.Maui.Controls.resources.dll.so => 27
	i64 u0x1c753b5ff15bce1b, ; 54: Mono.Android.Runtime.dll => 153
	i64 u0x1e3d87657e9659bc, ; 55: Xamarin.AndroidX.Navigation.UI => 89
	i64 u0x1e71143913d56c10, ; 56: lib-ko-Microsoft.Maui.Controls.resources.dll.so => 16
	i64 u0x1ed8fcce5e9b50a0, ; 57: Microsoft.Extensions.Options.dll => 47
	i64 u0x209375905fcc1bad, ; 58: lib_System.IO.Compression.Brotli.dll.so => 117
	i64 u0x2174319c0d835bc9, ; 59: System.Runtime => 137
	i64 u0x220fd4f2e7c48170, ; 60: th/Microsoft.Maui.Controls.resources => 27
	i64 u0x2347c268e3e4e536, ; 61: Xamarin.GooglePlayServices.Basement.dll => 99
	i64 u0x237be844f1f812c7, ; 62: System.Threading.Thread.dll => 143
	i64 u0x2407aef2bbe8fadf, ; 63: System.Console => 113
	i64 u0x240abe014b27e7d3, ; 64: Xamarin.AndroidX.Core.dll => 76
	i64 u0x252073cc3caa62c2, ; 65: fr/Microsoft.Maui.Controls.resources.dll => 8
	i64 u0x25a0a7eff76ea08e, ; 66: SQLitePCLRaw.batteries_v2.dll => 61
	i64 u0x25e1850d10cdc8f7, ; 67: lib_Xamarin.AndroidX.Camera.Camera2.dll.so => 68
	i64 u0x2662c629b96b0b30, ; 68: lib_Xamarin.Kotlin.StdLib.dll.so => 101
	i64 u0x268c1439f13bcc29, ; 69: lib_Microsoft.Extensions.Primitives.dll.so => 48
	i64 u0x268f1dca6d06d437, ; 70: Xamarin.AndroidX.Camera.Core => 69
	i64 u0x273f3515de5faf0d, ; 71: id/Microsoft.Maui.Controls.resources.dll => 13
	i64 u0x2742545f9094896d, ; 72: hr/Microsoft.Maui.Controls.resources => 11
	i64 u0x27b410442fad6cf1, ; 73: Java.Interop.dll => 152
	i64 u0x2801845a2c71fbfb, ; 74: System.Net.Primitives.dll => 125
	i64 u0x2927d345f3daec35, ; 75: SkiaSharp.dll => 56
	i64 u0x2a128783efe70ba0, ; 76: uk/Microsoft.Maui.Controls.resources.dll => 29
	i64 u0x2a6507a5ffabdf28, ; 77: System.Diagnostics.TraceSource.dll => 115
	i64 u0x2ad156c8e1354139, ; 78: fi/Microsoft.Maui.Controls.resources => 7
	i64 u0x2af298f63581d886, ; 79: System.Text.RegularExpressions.dll => 142
	i64 u0x2afc1c4f898552ee, ; 80: lib_System.Formats.Asn1.dll.so => 116
	i64 u0x2b148910ed40fbf9, ; 81: zh-Hant/Microsoft.Maui.Controls.resources.dll => 33
	i64 u0x2c8bd14bb93a7d82, ; 82: lib-pl-Microsoft.Maui.Controls.resources.dll.so => 20
	i64 u0x2cd723e9fe623c7c, ; 83: lib_System.Private.Xml.Linq.dll.so => 131
	i64 u0x2d169d318a968379, ; 84: System.Threading.dll => 144
	i64 u0x2d47774b7d993f59, ; 85: sv/Microsoft.Maui.Controls.resources.dll => 26
	i64 u0x2db915caf23548d2, ; 86: System.Text.Json.dll => 141
	i64 u0x2e6f1f226821322a, ; 87: el/Microsoft.Maui.Controls.resources.dll => 5
	i64 u0x2f2e98e1c89b1aff, ; 88: System.Xml.ReaderWriter => 147
	i64 u0x309ee9eeec09a71e, ; 89: lib_Xamarin.AndroidX.Fragment.dll.so => 80
	i64 u0x31195fef5d8fb552, ; 90: _Microsoft.Android.Resource.Designer.dll => 34
	i64 u0x32243413e774362a, ; 91: Xamarin.AndroidX.CardView.dll => 73
	i64 u0x326256f7722d4fe5, ; 92: SkiaSharp.Views.Maui.Controls.dll => 58
	i64 u0x329753a17a517811, ; 93: fr/Microsoft.Maui.Controls.resources => 8
	i64 u0x32aa989ff07a84ff, ; 94: lib_System.Xml.ReaderWriter.dll.so => 147
	i64 u0x33829542f112d59b, ; 95: System.Collections.Immutable => 106
	i64 u0x33a31443733849fe, ; 96: lib-es-Microsoft.Maui.Controls.resources.dll.so => 6
	i64 u0x341abc357fbb4ebf, ; 97: lib_System.Net.Sockets.dll.so => 127
	i64 u0x34dfd74fe2afcf37, ; 98: Microsoft.Maui => 51
	i64 u0x34e292762d9615df, ; 99: cs/Microsoft.Maui.Controls.resources.dll => 2
	i64 u0x3508234247f48404, ; 100: Microsoft.Maui.Controls => 49
	i64 u0x3549870798b4cd30, ; 101: lib_Xamarin.AndroidX.ViewPager2.dll.so => 94
	i64 u0x355282fc1c909694, ; 102: Microsoft.Extensions.Configuration => 41
	i64 u0x380134e03b1e160a, ; 103: System.Collections.Immutable.dll => 106
	i64 u0x385c17636bb6fe6e, ; 104: Xamarin.AndroidX.CustomView.dll => 78
	i64 u0x38869c811d74050e, ; 105: System.Net.NameResolution.dll => 124
	i64 u0x393c226616977fdb, ; 106: lib_Xamarin.AndroidX.ViewPager.dll.so => 93
	i64 u0x395e37c3334cf82a, ; 107: lib-ca-Microsoft.Maui.Controls.resources.dll.so => 1
	i64 u0x3a5e80f61b57438b, ; 108: InTheHand.AndroidActivity => 39
	i64 u0x3b860f9932505633, ; 109: lib_System.Text.Encoding.Extensions.dll.so => 139
	i64 u0x3c7c495f58ac5ee9, ; 110: Xamarin.Kotlin.StdLib => 101
	i64 u0x3d1c50cc001a991e, ; 111: Xamarin.Google.Guava.ListenableFuture.dll => 96
	i64 u0x3d46f0b995082740, ; 112: System.Xml.Linq => 146
	i64 u0x3d9c2a242b040a50, ; 113: lib_Xamarin.AndroidX.Core.dll.so => 76
	i64 u0x3da7781d6333a8fe, ; 114: SQLitePCLRaw.batteries_v2 => 61
	i64 u0x407a10bb4bf95829, ; 115: lib_Xamarin.AndroidX.Navigation.Common.dll.so => 86
	i64 u0x40c6d9cbfdb8b9f7, ; 116: SkiaSharp.Views.Maui.Core.dll => 59
	i64 u0x41cab042be111c34, ; 117: lib_Xamarin.AndroidX.AppCompat.AppCompatResources.dll.so => 67
	i64 u0x43375950ec7c1b6a, ; 118: netstandard.dll => 150
	i64 u0x434c4e1d9284cdae, ; 119: Mono.Android.dll => 154
	i64 u0x43950f84de7cc79a, ; 120: pl/Microsoft.Maui.Controls.resources.dll => 20
	i64 u0x4515080865a951a5, ; 121: Xamarin.Kotlin.StdLib.dll => 101
	i64 u0x458de11ccdd97885, ; 122: lib_InTheHand.Net.Bluetooth.dll.so => 40
	i64 u0x45c40276a42e283e, ; 123: System.Diagnostics.TraceSource => 115
	i64 u0x46a4213bc97fe5ae, ; 124: lib-ru-Microsoft.Maui.Controls.resources.dll.so => 24
	i64 u0x47358bd471172e1d, ; 125: lib_System.Xml.Linq.dll.so => 146
	i64 u0x477d591f9fe3587b, ; 126: Maui.GoogleMaps => 54
	i64 u0x47daf4e1afbada10, ; 127: pt/Microsoft.Maui.Controls.resources => 22
	i64 u0x49e952f19a4e2022, ; 128: System.ObjectModel => 129
	i64 u0x4a5667b2462a664b, ; 129: lib_Xamarin.AndroidX.Navigation.UI.dll.so => 89
	i64 u0x4b7b6532ded934b7, ; 130: System.Text.Json => 141
	i64 u0x4bf547f87e5016a8, ; 131: lib_SkiaSharp.Views.Android.dll.so => 57
	i64 u0x4c9caee94c082049, ; 132: Xamarin.GooglePlayServices.Maps => 100
	i64 u0x4cc5f15266470798, ; 133: lib_Xamarin.AndroidX.Loader.dll.so => 85
	i64 u0x4d479f968a05e504, ; 134: System.Linq.Expressions.dll => 120
	i64 u0x4d55a010ffc4faff, ; 135: System.Private.Xml => 132
	i64 u0x4d95fccc1f67c7ca, ; 136: System.Runtime.Loader.dll => 135
	i64 u0x4dcf44c3c9b076a2, ; 137: it/Microsoft.Maui.Controls.resources.dll => 14
	i64 u0x4dd9247f1d2c3235, ; 138: Xamarin.AndroidX.Loader.dll => 85
	i64 u0x4e32f00cb0937401, ; 139: Mono.Android.Runtime => 153
	i64 u0x4f21ee6ef9eb527e, ; 140: ca/Microsoft.Maui.Controls.resources => 1
	i64 u0x4fd143768327dbba, ; 141: TerraMarcadaV2 => 104
	i64 u0x4fd5f3ee53d0a4f0, ; 142: SQLitePCLRaw.lib.e_sqlite3.android => 63
	i64 u0x4fe4a08392a99ac0, ; 143: lib_CommunityToolkit.Maui.Camera.dll.so => 36
	i64 u0x5037f0be3c28c7a3, ; 144: lib_Microsoft.Maui.Controls.dll.so => 49
	i64 u0x5112ed116d87baf8, ; 145: CommunityToolkit.Mvvm => 38
	i64 u0x5131bbe80989093f, ; 146: Xamarin.AndroidX.Lifecycle.ViewModel.Android.dll => 83
	i64 u0x526ce79eb8e90527, ; 147: lib_System.Net.Primitives.dll.so => 125
	i64 u0x529ffe06f39ab8db, ; 148: Xamarin.AndroidX.Core => 76
	i64 u0x52ff996554dbf352, ; 149: Microsoft.Maui.Graphics => 53
	i64 u0x535f7e40e8fef8af, ; 150: lib-sk-Microsoft.Maui.Controls.resources.dll.so => 25
	i64 u0x53be1038a61e8d44, ; 151: System.Runtime.InteropServices.RuntimeInformation.dll => 133
	i64 u0x53c3014b9437e684, ; 152: lib-zh-HK-Microsoft.Maui.Controls.resources.dll.so => 31
	i64 u0x54795225dd1587af, ; 153: lib_System.Runtime.dll.so => 137
	i64 u0x556e8b63b660ab8b, ; 154: Xamarin.AndroidX.Lifecycle.Common.Jvm.dll => 81
	i64 u0x5588627c9a108ec9, ; 155: System.Collections.Specialized => 108
	i64 u0x561449e1215a61e4, ; 156: lib_SkiaSharp.Views.Maui.Core.dll.so => 59
	i64 u0x571c5cfbec5ae8e2, ; 157: System.Private.Uri => 130
	i64 u0x578cd35c91d7b347, ; 158: lib_SQLitePCLRaw.core.dll.so => 62
	i64 u0x579a06fed6eec900, ; 159: System.Private.CoreLib.dll => 151
	i64 u0x57c542c14049b66d, ; 160: System.Diagnostics.DiagnosticSource => 114
	i64 u0x58601b2dda4a27b9, ; 161: lib-ja-Microsoft.Maui.Controls.resources.dll.so => 15
	i64 u0x58688d9af496b168, ; 162: Microsoft.Extensions.DependencyInjection.dll => 43
	i64 u0x5a89a886ae30258d, ; 163: lib_Xamarin.AndroidX.CoordinatorLayout.dll.so => 75
	i64 u0x5a8f6699f4a1caa9, ; 164: lib_System.Threading.dll.so => 144
	i64 u0x5ae9cd33b15841bf, ; 165: System.ComponentModel => 112
	i64 u0x5b5ba1327561f926, ; 166: lib_SkiaSharp.Views.Maui.Controls.dll.so => 58
	i64 u0x5b5f0e240a06a2a2, ; 167: da/Microsoft.Maui.Controls.resources.dll => 3
	i64 u0x5b755276902c8414, ; 168: Xamarin.GooglePlayServices.Base => 98
	i64 u0x5c393624b8176517, ; 169: lib_Microsoft.Extensions.Logging.dll.so => 45
	i64 u0x5db0cbbd1028510e, ; 170: lib_System.Runtime.InteropServices.dll.so => 134
	i64 u0x5db30905d3e5013b, ; 171: Xamarin.AndroidX.Collection.Jvm.dll => 74
	i64 u0x5e467bc8f09ad026, ; 172: System.Collections.Specialized.dll => 108
	i64 u0x5ea92fdb19ec8c4c, ; 173: System.Text.Encodings.Web.dll => 140
	i64 u0x5eb8046dd40e9ac3, ; 174: System.ComponentModel.Primitives => 110
	i64 u0x5f36ccf5c6a57e24, ; 175: System.Xml.ReaderWriter.dll => 147
	i64 u0x5f7399e166075632, ; 176: lib_SQLitePCLRaw.lib.e_sqlite3.android.dll.so => 63
	i64 u0x5f9a2d823f664957, ; 177: lib-el-Microsoft.Maui.Controls.resources.dll.so => 5
	i64 u0x609f4b7b63d802d4, ; 178: lib_Microsoft.Extensions.DependencyInjection.dll.so => 43
	i64 u0x60cd4e33d7e60134, ; 179: Xamarin.KotlinX.Coroutines.Core.Jvm => 102
	i64 u0x60f62d786afcf130, ; 180: System.Memory => 122
	i64 u0x614c1b64d506e796, ; 181: TerraMarcadaV2.dll => 104
	i64 u0x61be8d1299194243, ; 182: Microsoft.Maui.Controls.Xaml => 50
	i64 u0x61d2cba29557038f, ; 183: de/Microsoft.Maui.Controls.resources => 4
	i64 u0x61d88f399afb2f45, ; 184: lib_System.Runtime.Loader.dll.so => 135
	i64 u0x622eef6f9e59068d, ; 185: System.Private.CoreLib => 151
	i64 u0x63f1f6883c1e23c2, ; 186: lib_System.Collections.Immutable.dll.so => 106
	i64 u0x6400f68068c1e9f1, ; 187: Xamarin.Google.Android.Material.dll => 95
	i64 u0x658f524e4aba7dad, ; 188: CommunityToolkit.Maui.dll => 35
	i64 u0x65ecac39144dd3cc, ; 189: Microsoft.Maui.Controls.dll => 49
	i64 u0x65ece51227bfa724, ; 190: lib_System.Runtime.Numerics.dll.so => 136
	i64 u0x6692e924eade1b29, ; 191: lib_System.Console.dll.so => 113
	i64 u0x66a4e5c6a3fb0bae, ; 192: lib_Xamarin.AndroidX.Lifecycle.ViewModel.Android.dll.so => 83
	i64 u0x66d13304ce1a3efa, ; 193: Xamarin.AndroidX.CursorAdapter => 77
	i64 u0x68558ec653afa616, ; 194: lib-da-Microsoft.Maui.Controls.resources.dll.so => 3
	i64 u0x68fbbbe2eb455198, ; 195: System.Formats.Asn1 => 116
	i64 u0x69063fc0ba8e6bdd, ; 196: he/Microsoft.Maui.Controls.resources.dll => 9
	i64 u0x6938a76e7814446f, ; 197: Maui.GoogleMaps.dll => 54
	i64 u0x699dffb2427a2d71, ; 198: SQLitePCLRaw.lib.e_sqlite3.android.dll => 63
	i64 u0x6a4d7577b2317255, ; 199: System.Runtime.InteropServices.dll => 134
	i64 u0x6ace3b74b15ee4a4, ; 200: nb/Microsoft.Maui.Controls.resources => 18
	i64 u0x6d12bfaa99c72b1f, ; 201: lib_Microsoft.Maui.Graphics.dll.so => 53
	i64 u0x6d79993361e10ef2, ; 202: Microsoft.Extensions.Primitives => 48
	i64 u0x6d86d56b84c8eb71, ; 203: lib_Xamarin.AndroidX.CursorAdapter.dll.so => 77
	i64 u0x6d9bea6b3e895cf7, ; 204: Microsoft.Extensions.Primitives.dll => 48
	i64 u0x6e25a02c3833319a, ; 205: lib_Xamarin.AndroidX.Navigation.Fragment.dll.so => 87
	i64 u0x6fd2265da78b93a4, ; 206: lib_Microsoft.Maui.dll.so => 51
	i64 u0x6fdfc7de82c33008, ; 207: cs/Microsoft.Maui.Controls.resources => 2
	i64 u0x70e99f48c05cb921, ; 208: tr/Microsoft.Maui.Controls.resources.dll => 28
	i64 u0x70fd3deda22442d2, ; 209: lib-nb-Microsoft.Maui.Controls.resources.dll.so => 18
	i64 u0x71a495ea3761dde8, ; 210: lib-it-Microsoft.Maui.Controls.resources.dll.so => 14
	i64 u0x71ad672adbe48f35, ; 211: System.ComponentModel.Primitives.dll => 110
	i64 u0x72b1fb4109e08d7b, ; 212: lib-hr-Microsoft.Maui.Controls.resources.dll.so => 11
	i64 u0x73e4ce94e2eb6ffc, ; 213: lib_System.Memory.dll.so => 122
	i64 u0x755a91767330b3d4, ; 214: lib_Microsoft.Extensions.Configuration.dll.so => 41
	i64 u0x76012e7334db86e5, ; 215: lib_Xamarin.AndroidX.SavedState.dll.so => 91
	i64 u0x76ca07b878f44da0, ; 216: System.Runtime.Numerics.dll => 136
	i64 u0x7728bffaddbd1f7e, ; 217: InTheHand.Net.Bluetooth.dll => 40
	i64 u0x780bc73597a503a9, ; 218: lib-ms-Microsoft.Maui.Controls.resources.dll.so => 17
	i64 u0x783606d1e53e7a1a, ; 219: th/Microsoft.Maui.Controls.resources.dll => 27
	i64 u0x78a45e51311409b6, ; 220: Xamarin.AndroidX.Fragment.dll => 80
	i64 u0x791c2cc9c9406c15, ; 221: lib_Maui.GoogleMaps.Clustering.dll.so => 55
	i64 u0x7adb8da2ac89b647, ; 222: fi/Microsoft.Maui.Controls.resources.dll => 7
	i64 u0x7bef86a4335c4870, ; 223: System.ComponentModel.TypeConverter => 111
	i64 u0x7c0820144cd34d6a, ; 224: sk/Microsoft.Maui.Controls.resources.dll => 25
	i64 u0x7c2a0bd1e0f988fc, ; 225: lib-de-Microsoft.Maui.Controls.resources.dll.so => 4
	i64 u0x7cb95ad2a929d044, ; 226: Xamarin.GooglePlayServices.Basement => 99
	i64 u0x7cc637f941f716d0, ; 227: CommunityToolkit.Maui.Core => 37
	i64 u0x7d649b75d580bb42, ; 228: ms/Microsoft.Maui.Controls.resources.dll => 17
	i64 u0x7d8ee2bdc8e3aad1, ; 229: System.Numerics.Vectors => 128
	i64 u0x7dfc3d6d9d8d7b70, ; 230: System.Collections => 109
	i64 u0x7e946809d6008ef2, ; 231: lib_System.ObjectModel.dll.so => 129
	i64 u0x7ecc13347c8fd849, ; 232: lib_System.ComponentModel.dll.so => 112
	i64 u0x7f00ddd9b9ca5a13, ; 233: Xamarin.AndroidX.ViewPager.dll => 93
	i64 u0x7f9351cd44b1273f, ; 234: Microsoft.Extensions.Configuration.Abstractions => 42
	i64 u0x7fbd557c99b3ce6f, ; 235: lib_Xamarin.AndroidX.Lifecycle.LiveData.Core.dll.so => 82
	i64 u0x80fa55b6d1b0be99, ; 236: SQLitePCLRaw.provider.e_sqlite3 => 64
	i64 u0x812c069d5cdecc17, ; 237: System.dll => 149
	i64 u0x81ab745f6c0f5ce6, ; 238: zh-Hant/Microsoft.Maui.Controls.resources => 33
	i64 u0x8277f2be6b5ce05f, ; 239: Xamarin.AndroidX.AppCompat => 66
	i64 u0x828f06563b30bc50, ; 240: lib_Xamarin.AndroidX.CardView.dll.so => 73
	i64 u0x82d3be9e0bb6a10b, ; 241: InTheHand.AndroidActivity.dll => 39
	i64 u0x82f6403342e12049, ; 242: uk/Microsoft.Maui.Controls.resources => 29
	i64 u0x83144699b312ad81, ; 243: SQLite-net.dll => 60
	i64 u0x83c14ba66c8e2b8c, ; 244: zh-Hans/Microsoft.Maui.Controls.resources => 32
	i64 u0x844ac8f64fd78edc, ; 245: Xamarin.AndroidX.Camera.View.dll => 72
	i64 u0x84f9060cc4a93c8f, ; 246: lib_SkiaSharp.dll.so => 56
	i64 u0x86a909228dc7657b, ; 247: lib-zh-Hant-Microsoft.Maui.Controls.resources.dll.so => 33
	i64 u0x86b3e00c36b84509, ; 248: Microsoft.Extensions.Configuration.dll => 41
	i64 u0x87c69b87d9283884, ; 249: lib_System.Threading.Thread.dll.so => 143
	i64 u0x87f6569b25707834, ; 250: System.IO.Compression.Brotli.dll => 117
	i64 u0x8842b3a5d2d3fb36, ; 251: Microsoft.Maui.Essentials => 52
	i64 u0x88bda98e0cffb7a9, ; 252: lib_Xamarin.KotlinX.Coroutines.Core.Jvm.dll.so => 102
	i64 u0x8930322c7bd8f768, ; 253: netstandard => 150
	i64 u0x897a606c9e39c75f, ; 254: lib_System.ComponentModel.Primitives.dll.so => 110
	i64 u0x89c5188089ec2cd5, ; 255: lib_System.Runtime.InteropServices.RuntimeInformation.dll.so => 133
	i64 u0x8ad229ea26432ee2, ; 256: Xamarin.AndroidX.Loader => 85
	i64 u0x8b4ff5d0fdd5faa1, ; 257: lib_System.Diagnostics.DiagnosticSource.dll.so => 114
	i64 u0x8b9ceca7acae3451, ; 258: lib-he-Microsoft.Maui.Controls.resources.dll.so => 9
	i64 u0x8d0f420977c2c1c7, ; 259: Xamarin.AndroidX.CursorAdapter.dll => 77
	i64 u0x8d7b8ab4b3310ead, ; 260: System.Threading => 144
	i64 u0x8da188285aadfe8e, ; 261: System.Collections.Concurrent => 105
	i64 u0x8ed807bfe9858dfc, ; 262: Xamarin.AndroidX.Navigation.Common => 86
	i64 u0x8ee08b8194a30f48, ; 263: lib-hi-Microsoft.Maui.Controls.resources.dll.so => 10
	i64 u0x8ef7601039857a44, ; 264: lib-ro-Microsoft.Maui.Controls.resources.dll.so => 23
	i64 u0x8ef9414937d93a0a, ; 265: SQLitePCLRaw.core.dll => 62
	i64 u0x8f32c6f611f6ffab, ; 266: pt/Microsoft.Maui.Controls.resources.dll => 22
	i64 u0x8f8829d21c8985a4, ; 267: lib-pt-BR-Microsoft.Maui.Controls.resources.dll.so => 21
	i64 u0x8fd27d934d7b3a55, ; 268: SQLitePCLRaw.core => 62
	i64 u0x90263f8448b8f572, ; 269: lib_System.Diagnostics.TraceSource.dll.so => 115
	i64 u0x903101b46fb73a04, ; 270: _Microsoft.Android.Resource.Designer => 34
	i64 u0x90393bd4865292f3, ; 271: lib_System.IO.Compression.dll.so => 118
	i64 u0x90634f86c5ebe2b5, ; 272: Xamarin.AndroidX.Lifecycle.ViewModel.Android => 83
	i64 u0x907b636704ad79ef, ; 273: lib_Microsoft.Maui.Controls.Xaml.dll.so => 50
	i64 u0x91418dc638b29e68, ; 274: lib_Xamarin.AndroidX.CustomView.dll.so => 78
	i64 u0x9157bd523cd7ed36, ; 275: lib_System.Text.Json.dll.so => 141
	i64 u0x91a74f07b30d37e2, ; 276: System.Linq.dll => 121
	i64 u0x91c7e28b683d07a7, ; 277: CommunityToolkit.Maui.Camera => 36
	i64 u0x91fa41a87223399f, ; 278: ca/Microsoft.Maui.Controls.resources.dll => 1
	i64 u0x93cfa73ab28d6e35, ; 279: ms/Microsoft.Maui.Controls.resources => 17
	i64 u0x944077d8ca3c6580, ; 280: System.IO.Compression.dll => 118
	i64 u0x95c6b36f5f5d7039, ; 281: Xamarin.AndroidX.Camera.Camera2 => 68
	i64 u0x95d757769563d0d3, ; 282: Xamarin.AndroidX.Camera.Lifecycle.dll => 70
	i64 u0x967fc325e09bfa8c, ; 283: es/Microsoft.Maui.Controls.resources => 6
	i64 u0x9732d8dbddea3d9a, ; 284: id/Microsoft.Maui.Controls.resources => 13
	i64 u0x978be80e5210d31b, ; 285: Microsoft.Maui.Graphics.dll => 53
	i64 u0x979ab54025cc1c7f, ; 286: lib_Xamarin.GooglePlayServices.Base.dll.so => 98
	i64 u0x97b8c771ea3e4220, ; 287: System.ComponentModel.dll => 112
	i64 u0x97e144c9d3c6976e, ; 288: System.Collections.Concurrent.dll => 105
	i64 u0x99052c1297204af4, ; 289: lib_Xamarin.AndroidX.Camera.Core.dll.so => 69
	i64 u0x991d510397f92d9d, ; 290: System.Linq.Expressions => 120
	i64 u0x999cb19e1a04ffd3, ; 291: CommunityToolkit.Mvvm.dll => 38
	i64 u0x99a00ca5270c6878, ; 292: Xamarin.AndroidX.Navigation.Runtime => 88
	i64 u0x99cdc6d1f2d3a72f, ; 293: ko/Microsoft.Maui.Controls.resources.dll => 16
	i64 u0x9d5dbcf5a48583fe, ; 294: lib_Xamarin.AndroidX.Activity.dll.so => 65
	i64 u0x9d74dee1a7725f34, ; 295: Microsoft.Extensions.Configuration.Abstractions.dll => 42
	i64 u0x9e4534b6adaf6e84, ; 296: nl/Microsoft.Maui.Controls.resources => 19
	i64 u0x9eaf1efdf6f7267e, ; 297: Xamarin.AndroidX.Navigation.Common.dll => 86
	i64 u0x9ef542cf1f78c506, ; 298: Xamarin.AndroidX.Lifecycle.LiveData.Core => 82
	i64 u0x9ff334e3cf272fd6, ; 299: lib_Xamarin.AndroidX.Camera.Lifecycle.dll.so => 70
	i64 u0xa0d8259f4cc284ec, ; 300: lib_System.Security.Cryptography.dll.so => 138
	i64 u0xa1440773ee9d341e, ; 301: Xamarin.Google.Android.Material => 95
	i64 u0xa1b9d7c27f47219f, ; 302: Xamarin.AndroidX.Navigation.UI.dll => 89
	i64 u0xa2572680829d2c7c, ; 303: System.IO.Pipelines.dll => 119
	i64 u0xa2beee74530fc01c, ; 304: SkiaSharp.Views.Android => 57
	i64 u0xa46aa1eaa214539b, ; 305: ko/Microsoft.Maui.Controls.resources => 16
	i64 u0xa4d20d2ff0563d26, ; 306: lib_CommunityToolkit.Mvvm.dll.so => 38
	i64 u0xa5e599d1e0524750, ; 307: System.Numerics.Vectors.dll => 128
	i64 u0xa5f1ba49b85dd355, ; 308: System.Security.Cryptography.dll => 138
	i64 u0xa67dbee13e1df9ca, ; 309: Xamarin.AndroidX.SavedState.dll => 91
	i64 u0xa68a420042bb9b1f, ; 310: Xamarin.AndroidX.DrawerLayout.dll => 79
	i64 u0xa78ce3745383236a, ; 311: Xamarin.AndroidX.Lifecycle.Common.Jvm => 81
	i64 u0xa7c31b56b4dc7b33, ; 312: hu/Microsoft.Maui.Controls.resources => 12
	i64 u0xa843f6095f0d247d, ; 313: Xamarin.GooglePlayServices.Base.dll => 98
	i64 u0xa964304b5631e28a, ; 314: CommunityToolkit.Maui.Core.dll => 37
	i64 u0xaa2219c8e3449ff5, ; 315: Microsoft.Extensions.Logging.Abstractions => 46
	i64 u0xaa443ac34067eeef, ; 316: System.Private.Xml.dll => 132
	i64 u0xaa52de307ef5d1dd, ; 317: System.Net.Http => 123
	i64 u0xaaaf86367285a918, ; 318: Microsoft.Extensions.DependencyInjection.Abstractions.dll => 44
	i64 u0xaaf84bb3f052a265, ; 319: el/Microsoft.Maui.Controls.resources => 5
	i64 u0xab9c1b2687d86b0b, ; 320: lib_System.Linq.Expressions.dll.so => 120
	i64 u0xac2af3fa195a15ce, ; 321: System.Runtime.Numerics => 136
	i64 u0xac5376a2a538dc10, ; 322: Xamarin.AndroidX.Lifecycle.LiveData.Core.dll => 82
	i64 u0xac98d31068e24591, ; 323: System.Xml.XDocument => 148
	i64 u0xacd46e002c3ccb97, ; 324: ro/Microsoft.Maui.Controls.resources => 23
	i64 u0xad89c07347f1bad6, ; 325: nl/Microsoft.Maui.Controls.resources.dll => 19
	i64 u0xadbb53caf78a79d2, ; 326: System.Web.HttpUtility => 145
	i64 u0xadc90ab061a9e6e4, ; 327: System.ComponentModel.TypeConverter.dll => 111
	i64 u0xae282bcd03739de7, ; 328: Java.Interop => 152
	i64 u0xae53579c90db1107, ; 329: System.ObjectModel.dll => 129
	i64 u0xae7ea18c61eef394, ; 330: SQLite-net => 60
	i64 u0xaebc1730fed5013c, ; 331: Maui.GoogleMaps.Clustering => 55
	i64 u0xafe29f45095518e7, ; 332: lib_Xamarin.AndroidX.Lifecycle.ViewModelSavedState.dll.so => 84
	i64 u0xb05cc42cd94c6d9d, ; 333: lib-sv-Microsoft.Maui.Controls.resources.dll.so => 26
	i64 u0xb220631954820169, ; 334: System.Text.RegularExpressions => 142
	i64 u0xb2a3f67f3bf29fce, ; 335: da/Microsoft.Maui.Controls.resources => 3
	i64 u0xb3f0a0fcda8d3ebc, ; 336: Xamarin.AndroidX.CardView => 73
	i64 u0xb46be1aa6d4fff93, ; 337: hi/Microsoft.Maui.Controls.resources => 10
	i64 u0xb477491be13109d8, ; 338: ar/Microsoft.Maui.Controls.resources => 0
	i64 u0xb4bd7015ecee9d86, ; 339: System.IO.Pipelines => 119
	i64 u0xb5c7fcdafbc67ee4, ; 340: Microsoft.Extensions.Logging.Abstractions.dll => 46
	i64 u0xb7b7753d1f319409, ; 341: sv/Microsoft.Maui.Controls.resources => 26
	i64 u0xb81a2c6e0aee50fe, ; 342: lib_System.Private.CoreLib.dll.so => 151
	i64 u0xb9f64d3b230def68, ; 343: lib-pt-Microsoft.Maui.Controls.resources.dll.so => 22
	i64 u0xb9fc3c8a556e3691, ; 344: ja/Microsoft.Maui.Controls.resources => 15
	i64 u0xba4670aa94a2b3c6, ; 345: lib_System.Xml.XDocument.dll.so => 148
	i64 u0xba48785529705af9, ; 346: System.Collections.dll => 109
	i64 u0xbb65706fde942ce3, ; 347: System.Net.Sockets => 127
	i64 u0xbc22a245dab70cb4, ; 348: lib_SQLitePCLRaw.provider.e_sqlite3.dll.so => 64
	i64 u0xbd0e2c0d55246576, ; 349: System.Net.Http.dll => 123
	i64 u0xbd437a2cdb333d0d, ; 350: Xamarin.AndroidX.ViewPager2 => 94
	i64 u0xbe532a80075c3dc8, ; 351: Xamarin.AndroidX.Camera.Core.dll => 69
	i64 u0xbee38d4a88835966, ; 352: Xamarin.AndroidX.AppCompat.AppCompatResources => 67
	i64 u0xc040a4ab55817f58, ; 353: ar/Microsoft.Maui.Controls.resources.dll => 0
	i64 u0xc0c3df43d4c04844, ; 354: Google.Maps.Utils.Android => 97
	i64 u0xc0d928351ab5ca77, ; 355: System.Console.dll => 113
	i64 u0xc12b8b3afa48329c, ; 356: lib_System.Linq.dll.so => 121
	i64 u0xc1ff9ae3cdb6e1e6, ; 357: Xamarin.AndroidX.Activity.dll => 65
	i64 u0xc28c50f32f81cc73, ; 358: ja/Microsoft.Maui.Controls.resources.dll => 15
	i64 u0xc2bcfec99f69365e, ; 359: Xamarin.AndroidX.ViewPager2.dll => 94
	i64 u0xc4d3858ed4d08512, ; 360: Xamarin.AndroidX.Lifecycle.ViewModelSavedState.dll => 84
	i64 u0xc50fded0ded1418c, ; 361: lib_System.ComponentModel.TypeConverter.dll.so => 111
	i64 u0xc519125d6bc8fb11, ; 362: lib_System.Net.Requests.dll.so => 126
	i64 u0xc5293b19e4dc230e, ; 363: Xamarin.AndroidX.Navigation.Fragment => 87
	i64 u0xc5325b2fcb37446f, ; 364: lib_System.Private.Xml.dll.so => 132
	i64 u0xc5a0f4b95a699af7, ; 365: lib_System.Private.Uri.dll.so => 130
	i64 u0xc62b529e23234872, ; 366: CommunityToolkit.Maui.Camera.dll => 36
	i64 u0xc7c01e7d7c93a110, ; 367: System.Text.Encoding.Extensions.dll => 139
	i64 u0xc7ce851898a4548e, ; 368: lib_System.Web.HttpUtility.dll.so => 145
	i64 u0xc858a28d9ee5a6c5, ; 369: lib_System.Collections.Specialized.dll.so => 108
	i64 u0xc87a188861588632, ; 370: Xamarin.AndroidX.Camera.Video.dll => 71
	i64 u0xc9e54b32fc19baf3, ; 371: lib_CommunityToolkit.Maui.dll.so => 35
	i64 u0xca3a723e7342c5b6, ; 372: lib-tr-Microsoft.Maui.Controls.resources.dll.so => 28
	i64 u0xcab3493c70141c2d, ; 373: pl/Microsoft.Maui.Controls.resources => 20
	i64 u0xcacfddc9f7c6de76, ; 374: ro/Microsoft.Maui.Controls.resources.dll => 23
	i64 u0xcbd4fdd9cef4a294, ; 375: lib__Microsoft.Android.Resource.Designer.dll.so => 34
	i64 u0xcc2876b32ef2794c, ; 376: lib_System.Text.RegularExpressions.dll.so => 142
	i64 u0xcc5c3bb714c4561e, ; 377: Xamarin.KotlinX.Coroutines.Core.Jvm.dll => 102
	i64 u0xcc76886e09b88260, ; 378: Xamarin.KotlinX.Serialization.Core.Jvm.dll => 103
	i64 u0xccf25c4b634ccd3a, ; 379: zh-Hans/Microsoft.Maui.Controls.resources.dll => 32
	i64 u0xcd10a42808629144, ; 380: System.Net.Requests => 126
	i64 u0xcdd0c48b6937b21c, ; 381: Xamarin.AndroidX.SwipeRefreshLayout => 92
	i64 u0xcf23d8093f3ceadf, ; 382: System.Diagnostics.DiagnosticSource.dll => 114
	i64 u0xcf8fc898f98b0d34, ; 383: System.Private.Xml.Linq => 131
	i64 u0xcfb21487d9cb358b, ; 384: Xamarin.GooglePlayServices.Maps.dll => 100
	i64 u0xd0352656223f7238, ; 385: lib_Google.Maps.Utils.Android.dll.so => 97
	i64 u0xd1194e1d8a8de83c, ; 386: lib_Xamarin.AndroidX.Lifecycle.Common.Jvm.dll.so => 81
	i64 u0xd3144156a3727ebe, ; 387: Xamarin.Google.Guava.ListenableFuture => 96
	i64 u0xd333d0af9e423810, ; 388: System.Runtime.InteropServices => 134
	i64 u0xd3426d966bb704f5, ; 389: Xamarin.AndroidX.AppCompat.AppCompatResources.dll => 67
	i64 u0xd3651b6fc3125825, ; 390: System.Private.Uri.dll => 130
	i64 u0xd373685349b1fe8b, ; 391: Microsoft.Extensions.Logging.dll => 45
	i64 u0xd3e4c8d6a2d5d470, ; 392: it/Microsoft.Maui.Controls.resources => 14
	i64 u0xd4645626dffec99d, ; 393: lib_Microsoft.Extensions.DependencyInjection.Abstractions.dll.so => 44
	i64 u0xd5507e11a2b2839f, ; 394: Xamarin.AndroidX.Lifecycle.ViewModelSavedState => 84
	i64 u0xd6694f8359737e4e, ; 395: Xamarin.AndroidX.SavedState => 91
	i64 u0xd6d21782156bc35b, ; 396: Xamarin.AndroidX.SwipeRefreshLayout.dll => 92
	i64 u0xd72329819cbbbc44, ; 397: lib_Microsoft.Extensions.Configuration.Abstractions.dll.so => 42
	i64 u0xd7b3764ada9d341d, ; 398: lib_Microsoft.Extensions.Logging.Abstractions.dll.so => 46
	i64 u0xda1dfa4c534a9251, ; 399: Microsoft.Extensions.DependencyInjection => 43
	i64 u0xdad05a11827959a3, ; 400: System.Collections.NonGeneric.dll => 107
	i64 u0xdb5383ab5865c007, ; 401: lib-vi-Microsoft.Maui.Controls.resources.dll.so => 30
	i64 u0xdb8f858873e2186b, ; 402: SkiaSharp.Views.Maui.Controls => 58
	i64 u0xdbeda89f832aa805, ; 403: vi/Microsoft.Maui.Controls.resources.dll => 30
	i64 u0xdbf9607a441b4505, ; 404: System.Linq => 121
	i64 u0xdce2c53525640bf3, ; 405: Microsoft.Extensions.Logging => 45
	i64 u0xdd2b722d78ef5f43, ; 406: System.Runtime.dll => 137
	i64 u0xdd67031857c72f96, ; 407: lib_System.Text.Encodings.Web.dll.so => 140
	i64 u0xdde30e6b77aa6f6c, ; 408: lib-zh-Hans-Microsoft.Maui.Controls.resources.dll.so => 32
	i64 u0xde110ae80fa7c2e2, ; 409: System.Xml.XDocument.dll => 148
	i64 u0xde8769ebda7d8647, ; 410: hr/Microsoft.Maui.Controls.resources.dll => 11
	i64 u0xe0142572c095a480, ; 411: Xamarin.AndroidX.AppCompat.dll => 66
	i64 u0xe02f89350ec78051, ; 412: Xamarin.AndroidX.CoordinatorLayout.dll => 75
	i64 u0xe192a588d4410686, ; 413: lib_System.IO.Pipelines.dll.so => 119
	i64 u0xe1a08bd3fa539e0d, ; 414: System.Runtime.Loader => 135
	i64 u0xe1b52f9f816c70ef, ; 415: System.Private.Xml.Linq.dll => 131
	i64 u0xe2420585aeceb728, ; 416: System.Net.Requests.dll => 126
	i64 u0xe29b73bc11392966, ; 417: lib-id-Microsoft.Maui.Controls.resources.dll.so => 13
	i64 u0xe32f7761c7b3a92f, ; 418: Maui.GoogleMaps.Clustering.dll => 55
	i64 u0xe3811d68d4fe8463, ; 419: pt-BR/Microsoft.Maui.Controls.resources.dll => 21
	i64 u0xe3a586956771a0ed, ; 420: lib_SQLite-net.dll.so => 60
	i64 u0xe494f7ced4ecd10a, ; 421: hu/Microsoft.Maui.Controls.resources.dll => 12
	i64 u0xe4a9b1e40d1e8917, ; 422: lib-fi-Microsoft.Maui.Controls.resources.dll.so => 7
	i64 u0xe5434e8a119ceb69, ; 423: lib_Mono.Android.dll.so => 154
	i64 u0xedc4817167106c23, ; 424: System.Net.Sockets.dll => 127
	i64 u0xedc632067fb20ff3, ; 425: System.Memory.dll => 122
	i64 u0xedc8e4ca71a02a8b, ; 426: Xamarin.AndroidX.Navigation.Runtime.dll => 88
	i64 u0xeeb7ebb80150501b, ; 427: lib_Xamarin.AndroidX.Collection.Jvm.dll.so => 74
	i64 u0xef602c523fe2e87a, ; 428: lib_Xamarin.Google.Guava.ListenableFuture.dll.so => 96
	i64 u0xef72742e1bcca27a, ; 429: Microsoft.Maui.Essentials.dll => 52
	i64 u0xefc053a0fcd90491, ; 430: lib_InTheHand.AndroidActivity.dll.so => 39
	i64 u0xefec0b7fdc57ec42, ; 431: Xamarin.AndroidX.Activity => 65
	i64 u0xf00c29406ea45e19, ; 432: es/Microsoft.Maui.Controls.resources.dll => 6
	i64 u0xf09e47b6ae914f6e, ; 433: System.Net.NameResolution => 124
	i64 u0xf11b621fc87b983f, ; 434: Microsoft.Maui.Controls.Xaml.dll => 50
	i64 u0xf1c4b4005493d871, ; 435: System.Formats.Asn1.dll => 116
	i64 u0xf238bd79489d3a96, ; 436: lib-nl-Microsoft.Maui.Controls.resources.dll.so => 19
	i64 u0xf2ad1f805f79a2af, ; 437: Google.Maps.Utils.Android.dll => 97
	i64 u0xf32a2fa88738a54c, ; 438: lib_Xamarin.AndroidX.Camera.Video.dll.so => 71
	i64 u0xf37221fda4ef8830, ; 439: lib_Xamarin.Google.Android.Material.dll.so => 95
	i64 u0xf3ddfe05336abf29, ; 440: System => 149
	i64 u0xf4727d423e5d26f3, ; 441: SkiaSharp => 56
	i64 u0xf4c1dd70a5496a17, ; 442: System.IO.Compression => 118
	i64 u0xf6077741019d7428, ; 443: Xamarin.AndroidX.CoordinatorLayout => 75
	i64 u0xf77b20923f07c667, ; 444: de/Microsoft.Maui.Controls.resources.dll => 4
	i64 u0xf7e2cac4c45067b3, ; 445: lib_System.Numerics.Vectors.dll.so => 128
	i64 u0xf7e74930e0e3d214, ; 446: zh-HK/Microsoft.Maui.Controls.resources.dll => 31
	i64 u0xf84773b5c81e3cef, ; 447: lib-uk-Microsoft.Maui.Controls.resources.dll.so => 29
	i64 u0xf8abd63acd77d37b, ; 448: Xamarin.AndroidX.Camera.View => 72
	i64 u0xf8e045dc345b2ea3, ; 449: lib_Xamarin.AndroidX.RecyclerView.dll.so => 90
	i64 u0xf915dc29808193a1, ; 450: System.Web.HttpUtility.dll => 145
	i64 u0xf96c777a2a0686f4, ; 451: hi/Microsoft.Maui.Controls.resources.dll => 10
	i64 u0xf9eec5bb3a6aedc6, ; 452: Microsoft.Extensions.Options => 47
	i64 u0xfa5ed7226d978949, ; 453: lib-ar-Microsoft.Maui.Controls.resources.dll.so => 0
	i64 u0xfa645d91e9fc4cba, ; 454: System.Threading.Thread => 143
	i64 u0xfa99d44ebf9bea5b, ; 455: SkiaSharp.Views.Maui.Core => 59
	i64 u0xfb022853d73b7fa5, ; 456: lib_SQLitePCLRaw.batteries_v2.dll.so => 61
	i64 u0xfbf0a31c9fc34bc4, ; 457: lib_System.Net.Http.dll.so => 123
	i64 u0xfc719aec26adf9d9, ; 458: Xamarin.AndroidX.Navigation.Fragment.dll => 87
	i64 u0xfd22f00870e40ae0, ; 459: lib_Xamarin.AndroidX.DrawerLayout.dll.so => 79
	i64 u0xfd49b3c1a76e2748, ; 460: System.Runtime.InteropServices.RuntimeInformation => 133
	i64 u0xfd536c702f64dc47, ; 461: System.Text.Encoding.Extensions => 139
	i64 u0xfd583f7657b6a1cb, ; 462: Xamarin.AndroidX.Fragment => 80
	i64 u0xfdbe4710aa9beeff, ; 463: CommunityToolkit.Maui => 35
	i64 u0xfeae9952cf03b8cb ; 464: tr/Microsoft.Maui.Controls.resources => 28
], align 8

@assembly_image_cache_indices = dso_local local_unnamed_addr constant [465 x i32] [
	i32 92, i32 88, i32 37, i32 153, i32 66, i32 64, i32 24, i32 2,
	i32 30, i32 125, i32 90, i32 109, i32 51, i32 31, i32 146, i32 74,
	i32 24, i32 107, i32 54, i32 79, i32 47, i32 107, i32 99, i32 138,
	i32 71, i32 25, i32 103, i32 93, i32 21, i32 70, i32 100, i32 154,
	i32 52, i32 124, i32 57, i32 78, i32 117, i32 68, i32 104, i32 90,
	i32 8, i32 152, i32 9, i32 44, i32 40, i32 150, i32 12, i32 140,
	i32 103, i32 18, i32 72, i32 105, i32 149, i32 27, i32 153, i32 89,
	i32 16, i32 47, i32 117, i32 137, i32 27, i32 99, i32 143, i32 113,
	i32 76, i32 8, i32 61, i32 68, i32 101, i32 48, i32 69, i32 13,
	i32 11, i32 152, i32 125, i32 56, i32 29, i32 115, i32 7, i32 142,
	i32 116, i32 33, i32 20, i32 131, i32 144, i32 26, i32 141, i32 5,
	i32 147, i32 80, i32 34, i32 73, i32 58, i32 8, i32 147, i32 106,
	i32 6, i32 127, i32 51, i32 2, i32 49, i32 94, i32 41, i32 106,
	i32 78, i32 124, i32 93, i32 1, i32 39, i32 139, i32 101, i32 96,
	i32 146, i32 76, i32 61, i32 86, i32 59, i32 67, i32 150, i32 154,
	i32 20, i32 101, i32 40, i32 115, i32 24, i32 146, i32 54, i32 22,
	i32 129, i32 89, i32 141, i32 57, i32 100, i32 85, i32 120, i32 132,
	i32 135, i32 14, i32 85, i32 153, i32 1, i32 104, i32 63, i32 36,
	i32 49, i32 38, i32 83, i32 125, i32 76, i32 53, i32 25, i32 133,
	i32 31, i32 137, i32 81, i32 108, i32 59, i32 130, i32 62, i32 151,
	i32 114, i32 15, i32 43, i32 75, i32 144, i32 112, i32 58, i32 3,
	i32 98, i32 45, i32 134, i32 74, i32 108, i32 140, i32 110, i32 147,
	i32 63, i32 5, i32 43, i32 102, i32 122, i32 104, i32 50, i32 4,
	i32 135, i32 151, i32 106, i32 95, i32 35, i32 49, i32 136, i32 113,
	i32 83, i32 77, i32 3, i32 116, i32 9, i32 54, i32 63, i32 134,
	i32 18, i32 53, i32 48, i32 77, i32 48, i32 87, i32 51, i32 2,
	i32 28, i32 18, i32 14, i32 110, i32 11, i32 122, i32 41, i32 91,
	i32 136, i32 40, i32 17, i32 27, i32 80, i32 55, i32 7, i32 111,
	i32 25, i32 4, i32 99, i32 37, i32 17, i32 128, i32 109, i32 129,
	i32 112, i32 93, i32 42, i32 82, i32 64, i32 149, i32 33, i32 66,
	i32 73, i32 39, i32 29, i32 60, i32 32, i32 72, i32 56, i32 33,
	i32 41, i32 143, i32 117, i32 52, i32 102, i32 150, i32 110, i32 133,
	i32 85, i32 114, i32 9, i32 77, i32 144, i32 105, i32 86, i32 10,
	i32 23, i32 62, i32 22, i32 21, i32 62, i32 115, i32 34, i32 118,
	i32 83, i32 50, i32 78, i32 141, i32 121, i32 36, i32 1, i32 17,
	i32 118, i32 68, i32 70, i32 6, i32 13, i32 53, i32 98, i32 112,
	i32 105, i32 69, i32 120, i32 38, i32 88, i32 16, i32 65, i32 42,
	i32 19, i32 86, i32 82, i32 70, i32 138, i32 95, i32 89, i32 119,
	i32 57, i32 16, i32 38, i32 128, i32 138, i32 91, i32 79, i32 81,
	i32 12, i32 98, i32 37, i32 46, i32 132, i32 123, i32 44, i32 5,
	i32 120, i32 136, i32 82, i32 148, i32 23, i32 19, i32 145, i32 111,
	i32 152, i32 129, i32 60, i32 55, i32 84, i32 26, i32 142, i32 3,
	i32 73, i32 10, i32 0, i32 119, i32 46, i32 26, i32 151, i32 22,
	i32 15, i32 148, i32 109, i32 127, i32 64, i32 123, i32 94, i32 69,
	i32 67, i32 0, i32 97, i32 113, i32 121, i32 65, i32 15, i32 94,
	i32 84, i32 111, i32 126, i32 87, i32 132, i32 130, i32 36, i32 139,
	i32 145, i32 108, i32 71, i32 35, i32 28, i32 20, i32 23, i32 34,
	i32 142, i32 102, i32 103, i32 32, i32 126, i32 92, i32 114, i32 131,
	i32 100, i32 97, i32 81, i32 96, i32 134, i32 67, i32 130, i32 45,
	i32 14, i32 44, i32 84, i32 91, i32 92, i32 42, i32 46, i32 43,
	i32 107, i32 30, i32 58, i32 30, i32 121, i32 45, i32 137, i32 140,
	i32 32, i32 148, i32 11, i32 66, i32 75, i32 119, i32 135, i32 131,
	i32 126, i32 13, i32 55, i32 21, i32 60, i32 12, i32 7, i32 154,
	i32 127, i32 122, i32 88, i32 74, i32 96, i32 52, i32 39, i32 65,
	i32 6, i32 124, i32 50, i32 116, i32 19, i32 97, i32 71, i32 95,
	i32 149, i32 56, i32 118, i32 75, i32 4, i32 128, i32 31, i32 29,
	i32 72, i32 90, i32 145, i32 10, i32 47, i32 0, i32 143, i32 59,
	i32 61, i32 123, i32 87, i32 79, i32 133, i32 139, i32 80, i32 35,
	i32 28
], align 4

@marshal_methods_number_of_classes = dso_local local_unnamed_addr constant i32 0, align 4

@marshal_methods_class_cache = dso_local local_unnamed_addr global [0 x %struct.MarshalMethodsManagedClass] zeroinitializer, align 8

; Names of classes in which marshal methods reside
@mm_class_names = dso_local local_unnamed_addr constant [0 x ptr] zeroinitializer, align 8

@mm_method_names = dso_local local_unnamed_addr constant [1 x %struct.MarshalMethodName] [
	%struct.MarshalMethodName {
		i64 u0x0000000000000000, ; name: 
		ptr @.MarshalMethodName.0_name; char* name
	} ; 0
], align 8

; get_function_pointer (uint32_t mono_image_index, uint32_t class_index, uint32_t method_token, void*& target_ptr)
@get_function_pointer = internal dso_local unnamed_addr global ptr null, align 8

; Functions

; Function attributes: memory(write, argmem: none, inaccessiblemem: none) "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" nofree norecurse nosync nounwind "stack-protector-buffer-size"="8" uwtable willreturn
define void @xamarin_app_init(ptr nocapture noundef readnone %env, ptr noundef %fn) local_unnamed_addr #0
{
	%fnIsNull = icmp eq ptr %fn, null
	br i1 %fnIsNull, label %1, label %2

1: ; preds = %0
	%putsResult = call noundef i32 @puts(ptr @.str.0)
	call void @abort()
	unreachable 

2: ; preds = %1, %0
	store ptr %fn, ptr @get_function_pointer, align 8, !tbaa !3
	ret void
}

; Strings
@.str.0 = private unnamed_addr constant [40 x i8] c"get_function_pointer MUST be specified\0A\00", align 1

;MarshalMethodName
@.MarshalMethodName.0_name = private unnamed_addr constant [1 x i8] c"\00", align 1

; External functions

; Function attributes: "no-trapping-math"="true" noreturn nounwind "stack-protector-buffer-size"="8"
declare void @abort() local_unnamed_addr #2

; Function attributes: nofree nounwind
declare noundef i32 @puts(ptr noundef) local_unnamed_addr #1
attributes #0 = { memory(write, argmem: none, inaccessiblemem: none) "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" nofree norecurse nosync nounwind "stack-protector-buffer-size"="8" "target-cpu"="generic" "target-features"="+fix-cortex-a53-835769,+neon,+outline-atomics,+v8a" uwtable willreturn }
attributes #1 = { nofree nounwind }
attributes #2 = { "no-trapping-math"="true" noreturn nounwind "stack-protector-buffer-size"="8" "target-cpu"="generic" "target-features"="+fix-cortex-a53-835769,+neon,+outline-atomics,+v8a" }

; Metadata
!llvm.module.flags = !{!0, !1, !7, !8, !9, !10}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!llvm.ident = !{!2}
!2 = !{!".NET for Android remotes/origin/release/9.0.1xx @ 9abff7703206541fdb83ffa80fe2c2753ad1997b"}
!3 = !{!4, !4, i64 0}
!4 = !{!"any pointer", !5, i64 0}
!5 = !{!"omnipotent char", !6, i64 0}
!6 = !{!"Simple C++ TBAA"}
!7 = !{i32 1, !"branch-target-enforcement", i32 0}
!8 = !{i32 1, !"sign-return-address", i32 0}
!9 = !{i32 1, !"sign-return-address-all", i32 0}
!10 = !{i32 1, !"sign-return-address-with-bkey", i32 0}
