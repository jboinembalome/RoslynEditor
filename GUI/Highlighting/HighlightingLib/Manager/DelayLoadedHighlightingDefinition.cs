namespace HighlightingLib.Manager
{
	using ICSharpCode.AvalonEdit.Highlighting;
	using System;
	using System.Collections.Generic;

	internal sealed class DelayLoadedHighlightingDefinition : IHighlightingDefinition
	{
		readonly object lockObj = new();
		readonly string _name;
		Func<IHighlightingDefinition> _lazyLoadingFunction;
		IHighlightingDefinition definition;
		Exception storedException;

		public DelayLoadedHighlightingDefinition(string name, Func<IHighlightingDefinition> lazyLoadingFunction)
		{
			_name = name;
			_lazyLoadingFunction = lazyLoadingFunction;
		}

		public string Name
		{
			get
			{
				if (_name != null)
					return _name;
				else
					return GetDefinition().Name;
			}
		}

		IHighlightingDefinition GetDefinition()
		{
			Func<IHighlightingDefinition> func;
			lock (lockObj)
			{
				if (definition != null)
					return definition;
				func = _lazyLoadingFunction;
			}
			Exception exception = null;
			IHighlightingDefinition def = null;
			try
			{
				using (var busyLock = BusyManager.Enter(this))
				{
					if (!busyLock.Success)
						throw new InvalidOperationException("Tried to create delay-loaded highlighting definition recursively. Make sure the are no cyclic references between the highlighting definitions.");
					def = func();
				}
				if (def == null)
					throw new InvalidOperationException("Function for delay-loading highlighting definition returned null");
			}
			catch (Exception ex)
			{
				exception = ex;
			}
			lock (lockObj)
			{
				_lazyLoadingFunction = null;
				if (definition == null && storedException == null)
				{
					definition = def;
					storedException = exception;
				}
				if (storedException != null)
					throw new HighlightingDefinitionInvalidException("Error delay-loading highlighting definition", storedException);
				return definition;
			}
		}

		public HighlightingRuleSet MainRuleSet
		{
			get
			{
				return GetDefinition().MainRuleSet;
			}
		}

		public HighlightingRuleSet GetNamedRuleSet(string name)
		{
			return GetDefinition().GetNamedRuleSet(name);
		}

		public HighlightingColor GetNamedColor(string name)
		{
			return GetDefinition().GetNamedColor(name);
		}

		public IEnumerable<HighlightingColor> NamedHighlightingColors
		{
			get
			{
				return GetDefinition().NamedHighlightingColors;
			}
		}

		public override string ToString()
		{
			return Name;
		}

		public IDictionary<string, string> Properties
		{
			get
			{
				return GetDefinition().Properties;
			}
		}
	}
}
