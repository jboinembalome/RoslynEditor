namespace HighlightingLib.Manager
{
	using System;
	using System.Diagnostics;
	using System.Reflection;
	using System.Runtime.Serialization;
	using System.Windows;
	using System.Windows.Media;
	using ICSharpCode.AvalonEdit.Highlighting;
	using ICSharpCode.AvalonEdit.Rendering;

	/// <summary>
	/// HighlightingBrush implementation that finds a brush using a resource.
	/// </summary>
	[Serializable]
	sealed class SystemColorHighlightingBrush : HighlightingBrush, ISerializable
	{
		readonly PropertyInfo _property;

		public SystemColorHighlightingBrush(PropertyInfo property)
		{
			Debug.Assert(property.ReflectedType == typeof(SystemColors));
			Debug.Assert(typeof(Brush).IsAssignableFrom(property.PropertyType));
			_property = property;
		}

		public override Brush GetBrush(ITextRunConstructionContext context)
		{
			return (Brush)_property.GetValue(null, null);
		}

		public override string ToString()
		{
			return _property.Name;
		}

		SystemColorHighlightingBrush(SerializationInfo info, StreamingContext context)
		{
			_property = typeof(SystemColors).GetProperty(info.GetString("propertyName"));
			if (_property == null)
				throw new ArgumentException("Error deserializing SystemColorHighlightingBrush");
		}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("propertyName", _property.Name);
		}

		public override bool Equals(object obj)
		{
            if (obj is not SystemColorHighlightingBrush other)
                return false;

            return object.Equals(_property, other._property);
		}

		public override int GetHashCode()
		{
			return _property.GetHashCode();
		}
	}

}
