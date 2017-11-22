using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Framework.Util.BaseTypeMethodExpend
{
	public static partial class Utils
	{
		// 数字操作

		#region 随机数
		/// <summary>生成随机数</summary>
		/// <returns>随机数</returns>
		public static int Rand()
		{
			byte[] _data = new byte[4];
			using(RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider())
				_rng.GetBytes(_data);
			int _num = BitConverter.ToInt32(_data, 0);
			_num = Math.Abs(_num);
			return _num;
		}

		/// <summary>生成随机数</summary>
		/// <param name="max">生成的随机数最大值</param>
		/// <returns>随机数</returns>
		public static int Rand(int max)
		{
			byte[] _data = new byte[4];
			using(RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider())
				_rng.GetBytes(_data);
			int _num = BitConverter.ToInt32(_data, 0) % (max + 1);
			_num = Math.Abs(_num);
			return _num;
		}

		/// <summary>生成随机数</summary>
		/// <param name="min">生成的随机数最小值</param>
		/// <param name="max">生成的随机数最大值</param>
		/// <returns>随机数</returns>
		public static int Rand(int min, int max)
		{
			return (Rand(max - min) + min);
		}
		#endregion

		#region 限制数字范围
		/// <summary>限制数字处于给出的范围中</summary>
		/// <param name="num">要限制的数字</param>
		/// <param name="num1">范围起点</param>
		/// <param name="num2">范围终点</param>
		/// <returns>位于范围内的数字</returns>
		public static int Limit_(this int num, int num1, int num2)
		{
			if(num1 == num2) return num1;
			if(num == num1 || num == num2) return num;

			int _min, _max;
			if(num1 < num2)
			{
				_min = num1;
				_max = num2;
			}
			else
			{
				_min = num2;
				_max = num1;
			}

			if(num < num1) return num1;
			if(num > num2) return num2;
			return num;
		}

		/// <summary>限制字符处于给出的范围中</summary>
		/// <param name="num">要限制的字符</param>
		/// <param name="num1">范围起点</param>
		/// <param name="num2">范围终点</param>
		/// <returns>位于范围内的数字</returns>
		public static char Limit_(this char num, char num1, char num2)
		{
			if(num1 == num2) return num1;
			if(num == num1 || num == num2) return num;

			char _min, _max;
			if(num1 < num2)
			{
				_min = num1;
				_max = num2;
			}
			else
			{
				_min = num2;
				_max = num1;
			}

			if(num < num1) return num1;
			if(num > num2) return num2;
			return num;
		}

		/// <summary>限制数字处于给出的范围中</summary>
		/// <param name="num">要限制的数字</param>
		/// <param name="num1">范围起点</param>
		/// <param name="num2">范围终点</param>
		/// <returns>位于范围内的数字</returns>
		public static byte Limit_(this byte num, byte num1, byte num2)
		{
			if(num1 == num2) return num1;
			if(num == num1 || num == num2) return num;

			byte _min, _max;
			if(num1 < num2)
			{
				_min = num1;
				_max = num2;
			}
			else
			{
				_min = num2;
				_max = num1;
			}

			if(num < num1) return num1;
			if(num > num2) return num2;
			return num;
		}

		/// <summary>限制数字处于给出的范围中</summary>
		/// <param name="num">要限制的数字</param>
		/// <param name="num1">范围起点</param>
		/// <param name="num2">范围终点</param>
		/// <returns>位于范围内的数字</returns>
		public static decimal Limit_(this decimal num, decimal num1, decimal num2)
		{
			if(num1 == num2) return num1;
			if(num == num1 || num == num2) return num;

			decimal _min, _max;
			if(num1 < num2)
			{
				_min = num1;
				_max = num2;
			}
			else
			{
				_min = num2;
				_max = num1;
			}

			if(num < num1) return num1;
			if(num > num2) return num2;
			return num;
		}

		/// <summary>限制数字处于给出的范围中</summary>
		/// <param name="num">要限制的数字</param>
		/// <param name="num1">范围起点</param>
		/// <param name="num2">范围终点</param>
		/// <returns>位于范围内的数字</returns>
		public static double Limit_(this double num, double num1, double num2)
		{
			if(num1 == num2) return num1;
			if(num == num1 || num == num2) return num;

			double _min, _max;
			if(num1 < num2)
			{
				_min = num1;
				_max = num2;
			}
			else
			{
				_min = num2;
				_max = num1;
			}

			if(num < num1) return num1;
			if(num > num2) return num2;
			return num;
		}

		/// <summary>限制数字处于给出的范围中</summary>
		/// <param name="num">要限制的数字</param>
		/// <param name="num1">范围起点</param>
		/// <param name="num2">范围终点</param>
		/// <returns>位于范围内的数字</returns>
		public static short Limit_(this short num, short num1, short num2)
		{
			if(num1 == num2) return num1;
			if(num == num1 || num == num2) return num;

			short _min, _max;
			if(num1 < num2)
			{
				_min = num1;
				_max = num2;
			}
			else
			{
				_min = num2;
				_max = num1;
			}

			if(num < num1) return num1;
			if(num > num2) return num2;
			return num;
		}

		/// <summary>限制数字处于给出的范围中</summary>
		/// <param name="num">要限制的数字</param>
		/// <param name="num1">范围起点</param>
		/// <param name="num2">范围终点</param>
		/// <returns>位于范围内的数字</returns>
		public static long Limit_(this long num, long num1, long num2)
		{
			if(num1 == num2) return num1;
			if(num == num1 || num == num2) return num;

			long _min, _max;
			if(num1 < num2)
			{
				_min = num1;
				_max = num2;
			}
			else
			{
				_min = num2;
				_max = num1;
			}

			if(num < num1) return num1;
			if(num > num2) return num2;
			return num;
		}

		/// <summary>限制数字处于给出的范围中</summary>
		/// <param name="num">要限制的数字</param>
		/// <param name="num1">范围起点</param>
		/// <param name="num2">范围终点</param>
		/// <returns>位于范围内的数字</returns>
		public static float Limit_(this float num, float num1, float num2)
		{
			if(num1 == num2) return num1;
			if(num == num1 || num == num2) return num;

			float _min, _max;
			if(num1 < num2)
			{
				_min = num1;
				_max = num2;
			}
			else
			{
				_min = num2;
				_max = num1;
			}

			if(num < num1) return num1;
			if(num > num2) return num2;
			return num;
		}
		#endregion

		#region 限制数字大小
		/// <summary>限制数字不小于指定值</summary>
		/// <param name="num">要限制的数字</param>
		/// <param name="value">指定的值</param>
		/// <returns>位于范围内的数字</returns>
		public static int NotLessThan_(this int num, int value)
		{
			return num < value ? value : num;
		}

		/// <summary>限制数字不大于指定值</summary>
		/// <param name="num">要限制的数字</param>
		/// <param name="value">指定的值</param>
		/// <returns>位于范围内的数字</returns>
		public static int NotGreaterThan_(this int num, int value)
		{
			return num > value ? value : num;
		}

		/// <summary>限制数字不小于指定值</summary>
		/// <param name="num">要限制的数字</param>
		/// <param name="value">指定的值</param>
		/// <returns>位于范围内的数字</returns>
		public static byte NotLessThan_(this byte num, byte value)
		{
			return num < value ? value : num;
		}

		/// <summary>限制数字不大于指定值</summary>
		/// <param name="num">要限制的数字</param>
		/// <param name="value">指定的值</param>
		/// <returns>位于范围内的数字</returns>
		public static byte NotGreaterThan_(this byte num, byte value)
		{
			return num > value ? value : num;
		}

		/// <summary>限制数字不小于指定值</summary>
		/// <param name="num">要限制的数字</param>
		/// <param name="value">指定的值</param>
		/// <returns>位于范围内的数字</returns>
		public static char NotLessThan_(this char num, char value)
		{
			return num < value ? value : num;
		}

		/// <summary>限制数字不大于指定值</summary>
		/// <param name="num">要限制的数字</param>
		/// <param name="value">指定的值</param>
		/// <returns>位于范围内的数字</returns>
		public static char NotGreaterThan_(this char num, char value)
		{
			return num > value ? value : num;
		}

		/// <summary>限制数字不小于指定值</summary>
		/// <param name="num">要限制的数字</param>
		/// <param name="value">指定的值</param>
		/// <returns>位于范围内的数字</returns>
		public static decimal NotLessThan_(this decimal num, decimal value)
		{
			return num < value ? value : num;
		}

		/// <summary>限制数字不大于指定值</summary>
		/// <param name="num">要限制的数字</param>
		/// <param name="value">指定的值</param>
		/// <returns>位于范围内的数字</returns>
		public static decimal NotGreaterThan_(this decimal num, decimal value)
		{
			return num > value ? value : num;
		}

		/// <summary>限制数字不小于指定值</summary>
		/// <param name="num">要限制的数字</param>
		/// <param name="value">指定的值</param>
		/// <returns>位于范围内的数字</returns>
		public static double NotLessThan_(this double num, double value)
		{
			return num < value ? value : num;
		}

		/// <summary>限制数字不大于指定值</summary>
		/// <param name="num">要限制的数字</param>
		/// <param name="value">指定的值</param>
		/// <returns>位于范围内的数字</returns>
		public static double NotGreaterThan_(this double num, double value)
		{
			return num > value ? value : num;
		}

		/// <summary>限制数字不小于指定值</summary>
		/// <param name="num">要限制的数字</param>
		/// <param name="value">指定的值</param>
		/// <returns>位于范围内的数字</returns>
		public static short NotLessThan_(this short num, short value)
		{
			return num < value ? value : num;
		}

		/// <summary>限制数字不大于指定值</summary>
		/// <param name="num">要限制的数字</param>
		/// <param name="value">指定的值</param>
		/// <returns>位于范围内的数字</returns>
		public static short NotGreaterThan_(this short num, short value)
		{
			return num > value ? value : num;
		}

		/// <summary>限制数字不小于指定值</summary>
		/// <param name="num">要限制的数字</param>
		/// <param name="value">指定的值</param>
		/// <returns>位于范围内的数字</returns>
		public static long NotLessThan_(this long num, long value)
		{
			return num < value ? value : num;
		}

		/// <summary>限制数字不大于指定值</summary>
		/// <param name="num">要限制的数字</param>
		/// <param name="value">指定的值</param>
		/// <returns>位于范围内的数字</returns>
		public static long NotGreaterThan_(this long num, long value)
		{
			return num > value ? value : num;
		}

		/// <summary>限制数字不小于指定值</summary>
		/// <param name="num">要限制的数字</param>
		/// <param name="value">指定的值</param>
		/// <returns>位于范围内的数字</returns>
		public static float NotLessThan_(this float num, float value)
		{
			return num < value ? value : num;
		}

		/// <summary>限制数字不大于指定值</summary>
		/// <param name="num">要限制的数字</param>
		/// <param name="value">指定的值</param>
		/// <returns>位于范围内的数字</returns>
		public static float NotGreaterThan_(this float num, float value)
		{
			return num > value ? value : num;
		}
		#endregion

		#region 验证数字范围
		/// <summary>验证数字是否位于范围内</summary>
		/// <param name="num">要验证的数字</param>
		/// <param name="num1">范围起点</param>
		/// <param name="num2">范围终点</param>
		/// <param name="include">是否包含范围值</param>
		/// <returns>指示是否位于范围内</returns>
		public static bool Between_(this int num, int num1, int num2, bool include = true)
		{
			int _min, _max;
			bool _reval;

			if(num1 < num2)
			{
				_min = num1;
				_max = num2;
			}
			else
			{
				_min = num2;
				_max = num1;
			}

			if(include)
				_reval = (num >= _min && num <= _max) ? true : false;
			else
				_reval = (num > _min && num < _max) ? true : false;

			return _reval;
		}

		/// <summary>验证数字是否位于范围内</summary>
		/// <param name="num">要验证的数字</param>
		/// <param name="num1">范围起点</param>
		/// <param name="num2">范围终点</param>
		/// <param name="include">是否包含范围值</param>
		/// <returns>指示是否位于范围内</returns>
		public static bool Between_(this byte num, byte num1, byte num2, bool include = true)
		{
			byte _min, _max;
			bool _reval;

			if(num1 < num2)
			{
				_min = num1;
				_max = num2;
			}
			else
			{
				_min = num2;
				_max = num1;
			}

			if(include)
				_reval = (num >= _min && num <= _max) ? true : false;
			else
				_reval = (num > _min && num < _max) ? true : false;

			return _reval;
		}

		/// <summary>验证数字是否位于范围内</summary>
		/// <param name="num">要验证的数字</param>
		/// <param name="num1">范围起点</param>
		/// <param name="num2">范围终点</param>
		/// <param name="include">是否包含范围值</param>
		/// <returns>指示是否位于范围内</returns>
		public static bool Between_(this decimal num, decimal num1, decimal num2, bool include = true)
		{
			decimal _min, _max;
			bool _reval;

			if(num1 < num2)
			{
				_min = num1;
				_max = num2;
			}
			else
			{
				_min = num2;
				_max = num1;
			}

			if(include)
				_reval = (num >= _min && num <= _max) ? true : false;
			else
				_reval = (num > _min && num < _max) ? true : false;

			return _reval;
		}

		/// <summary>验证数字是否位于范围内</summary>
		/// <param name="num">要验证的数字</param>
		/// <param name="num1">范围起点</param>
		/// <param name="num2">范围终点</param>
		/// <param name="include">是否包含范围值</param>
		/// <returns>指示是否位于范围内</returns>
		public static bool Between_(this double num, double num1, double num2, bool include = true)
		{
			double _min, _max;
			bool _reval;

			if(num1 < num2)
			{
				_min = num1;
				_max = num2;
			}
			else
			{
				_min = num2;
				_max = num1;
			}

			if(include)
				_reval = (num >= _min && num <= _max) ? true : false;
			else
				_reval = (num > _min && num < _max) ? true : false;

			return _reval;
		}

		/// <summary>验证数字是否位于范围内</summary>
		/// <param name="num">要验证的数字</param>
		/// <param name="num1">范围起点</param>
		/// <param name="num2">范围终点</param>
		/// <param name="include">是否包含范围值</param>
		/// <returns>指示是否位于范围内</returns>
		public static bool Between_(this short num, short num1, short num2, bool include = true)
		{
			short _min, _max;
			bool _reval;

			if(num1 < num2)
			{
				_min = num1;
				_max = num2;
			}
			else
			{
				_min = num2;
				_max = num1;
			}

			if(include)
				_reval = (num >= _min && num <= _max) ? true : false;
			else
				_reval = (num > _min && num < _max) ? true : false;

			return _reval;
		}

		/// <summary>验证数字是否位于范围内</summary>
		/// <param name="num">要验证的数字</param>
		/// <param name="num1">范围起点</param>
		/// <param name="num2">范围终点</param>
		/// <param name="include">是否包含范围值</param>
		/// <returns>指示是否位于范围内</returns>
		public static bool Between_(this long num, long num1, long num2, bool include = true)
		{
			long _min, _max;
			bool _reval;

			if(num1 < num2)
			{
				_min = num1;
				_max = num2;
			}
			else
			{
				_min = num2;
				_max = num1;
			}

			if(include)
				_reval = (num >= _min && num <= _max) ? true : false;
			else
				_reval = (num > _min && num < _max) ? true : false;

			return _reval;
		}

		/// <summary>验证数字是否位于范围内</summary>
		/// <param name="num">要验证的数字</param>
		/// <param name="num1">范围起点</param>
		/// <param name="num2">范围终点</param>
		/// <param name="include">是否包含范围值</param>
		/// <returns>指示是否位于范围内</returns>
		public static bool Between_(this float num, float num1, float num2, bool include = true)
		{
			float _min, _max;
			bool _reval;

			if(num1 < num2)
			{
				_min = num1;
				_max = num2;
			}
			else
			{
				_min = num2;
				_max = num1;
			}

			if(include)
				_reval = (num >= _min && num <= _max) ? true : false;
			else
				_reval = (num > _min && num < _max) ? true : false;

			return _reval;
		}

		/// <summary>验证数字是否位于范围内</summary>
		/// <param name="num">要验证的数字</param>
		/// <param name="num1">范围起点</param>
		/// <param name="num2">范围终点</param>
		/// <param name="include">是否包含范围值</param>
		/// <returns>指示是否位于范围内</returns>
		public static bool Between_(this char num, char num1, char num2, bool include = true)
		{
			char _min, _max;
			bool _reval;

			if(num1 < num2)
			{
				_min = num1;
				_max = num2;
			}
			else
			{
				_min = num2;
				_max = num1;
			}

			if(include)
				_reval = (num >= _min && num <= _max) ? true : false;
			else
				_reval = (num > _min && num < _max) ? true : false;

			return _reval;
		}
		#endregion

		#region 分页页码相关
		 
		#endregion
	}
}