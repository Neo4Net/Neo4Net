/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Configuration
{

	/// <summary>
	/// A configuration option with its active value.
	/// </summary>
	public class ConfigValue
	{
		 private readonly string _name;
		 private readonly Optional<string> _description;
		 private readonly Optional<string> _documentedDefaultValue;
		 private readonly Optional<object> _value;
		 private readonly string _valueDescription;
		 private readonly bool @internal;
		 private readonly bool _secret;
		 private readonly bool _dynamic;
		 private readonly bool _deprecated;
		 private readonly Optional<string> _replacement;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public ConfigValue(@Nonnull String name, @Nonnull Optional<String> description, @Nonnull Optional<String> documentedDefaultValue, @Nonnull Optional<Object> value, @Nonnull String valueDescription, boolean internal, boolean dynamic, boolean deprecated, @Nonnull Optional<String> replacement, boolean secret)
		 public ConfigValue( string name, Optional<string> description, Optional<string> documentedDefaultValue, Optional<object> value, string valueDescription, bool @internal, bool dynamic, bool deprecated, Optional<string> replacement, bool secret )
		 {
			  this._name = name;
			  this._description = description;
			  this._documentedDefaultValue = documentedDefaultValue;
			  this._value = value;
			  this._valueDescription = valueDescription;
			  this.@internal = @internal;
			  this._secret = secret;
			  this._dynamic = dynamic;
			  this._deprecated = deprecated;
			  this._replacement = replacement;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public String name()
		 public virtual string Name()
		 {
			  return _name;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public java.util.Optional<String> description()
		 public virtual Optional<string> Description()
		 {
			  return _description;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public java.util.Optional<Object> value()
		 public virtual Optional<object> Value()
		 {
			  return _value;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public java.util.Optional<String> valueAsString()
		 public virtual Optional<string> ValueAsString()
		 {
			  return this.Secret() ? Secret.OBSFUCATED : _value.map(ConfigValue.valueToString);
		 }

		 public override string ToString()
		 {
			  return ValueAsString().orElse("null");
		 }

		 public virtual bool Deprecated()
		 {
			  return _deprecated;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public java.util.Optional<String> replacement()
		 public virtual Optional<string> Replacement()
		 {
			  return _replacement;
		 }

		 public virtual bool Internal()
		 {
			  return @internal;
		 }

		 public virtual bool Secret()
		 {
			  return _secret;
		 }

		 public virtual bool Dynamic()
		 {
			  return _dynamic;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public java.util.Optional<String> documentedDefaultValue()
		 public virtual Optional<string> DocumentedDefaultValue()
		 {
			  return _documentedDefaultValue;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public String valueDescription()
		 public virtual string ValueDescription()
		 {
			  return _valueDescription;
		 }

		 internal static string ValueToString( object v )
		 {
			  if ( v is Duration )
			  {
					Duration d = ( Duration ) v;
					return string.Format( "{0:D}ms", d.toMillis() );
			  }
			  return v.ToString();
		 }
	}

}