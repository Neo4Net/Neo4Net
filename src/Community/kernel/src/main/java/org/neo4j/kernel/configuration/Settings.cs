using System;
using System.Collections.Generic;
using System.Text;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.configuration
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;
	using StringUtils = org.apache.commons.lang3.StringUtils;


	using Neo4Net.Graphdb.config;
	using Configuration = Neo4Net.Graphdb.config.Configuration;
	using InvalidSettingException = Neo4Net.Graphdb.config.InvalidSettingException;
	using Neo4Net.Graphdb.config;
	using Neo4Net.Graphdb.config;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;
	using Numbers = Neo4Net.Helpers.Numbers;
	using SocketAddressParser = Neo4Net.Helpers.SocketAddressParser;
	using TimeUtil = Neo4Net.Helpers.TimeUtil;
	using CollectorsUtil = Neo4Net.Helpers.Collection.CollectorsUtil;
	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using ByteUnit = Neo4Net.Io.ByteUnit;
	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Character.isDigit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.parseLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.default_advertised_address;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.default_listen_address;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.fs.FileUtils.fixSeparatorsInPath;

	/// <summary>
	/// Create settings for configurations in Neo4j. See <seealso cref="org.neo4j.graphdb.factory.GraphDatabaseSettings"/> for
	/// example.
	/// 
	/// <para>Each setting has a name, a parser that converts a string to the type of the setting, a default value,
	/// an optional inherited setting, and optional value converters/constraints.
	/// 
	/// </para>
	/// <para>A parser is a function that takes a string and converts to some type T. The parser may throw {@link
	/// IllegalArgumentException} if it fails.
	/// 
	/// </para>
	/// <para>The default value is the string representation of what you want as default. Special constants NO_DEFAULT, which means
	/// that you don't want any default value at all, can be used if no appropriate default value exists.
	/// </para>
	/// </summary>
	public class Settings
	{
		 private const string MATCHES_PATTERN_MESSAGE = "matches the pattern `%s`";

		 private interface SettingHelper<T> : Setting<T>
		 {
			  string Lookup( System.Func<string, string> settings );

			  string DefaultLookup( System.Func<string, string> settings );
		 }

		 public const string NO_DEFAULT = null;
		 public const string EMPTY = "";

		 public const string TRUE = "true";
		 public const string FALSE = "false";

		 public const string DEFAULT = "default";

		 public const string SEPARATOR = ",";

		 private const string SIZE_FORMAT = "\\d+[kmgKMG]?";

		 private static readonly string _sizeUnits = Arrays.ToString( StringHelper.SubstringSpecial( SIZE_FORMAT, SIZE_FORMAT.IndexOf( '[' ) + 1, SIZE_FORMAT.IndexOf( ']' ) ).ToCharArray() ).replace("[", "").replace("]", "");

		 public const string ANY = ".+";

		 /// <summary>
		 /// Helper class to build a <seealso cref="Setting"/>. A setting always have a name, a parser and a default value.
		 /// </summary>
		 /// @param <T> The concrete type of the setting that is being build </param>
		 public sealed class SettingBuilder<T>
		 {
			  internal readonly string Name;
			  internal readonly System.Func<string, T> Parser;
			  internal readonly string DefaultValue;
			  internal Setting<T> InheritedSetting;
			  internal IList<System.Func<T, System.Func<string, string>, T>> ValueConstraints;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: private SettingBuilder(@Nonnull final String name, @Nonnull final java.util.function.Function<String,T> parser, @Nullable final String defaultValue)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			  internal SettingBuilder( string name, System.Func<string, T> parser, string defaultValue )
			  {
					this.Name = name;
					this.Parser = parser;
					this.DefaultValue = defaultValue;
			  }

			  /// <summary>
			  /// Setup a class to inherit from. Both the default value and the actual user supplied value will be inherited.
			  /// Limited to one parent, but chains are allowed and works as expected by going up on level until a valid value
			  /// is found.
			  /// </summary>
			  /// <param name="inheritedSetting"> the setting to inherit value and default value from. </param>
			  /// <exception cref="AssertionError"> if more than one inheritance is provided. </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public SettingBuilder<T> inherits(@Nonnull final org.neo4j.graphdb.config.Setting<T> inheritedSetting)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			  public SettingBuilder<T> Inherits( Setting<T> inheritedSetting )
			  {
					// Make sure we only inherits from one other setting
					if ( this.InheritedSetting != null )
					{
						 throw new AssertionError( "Can only inherit from one setting" );
					}

					this.InheritedSetting = inheritedSetting;
					return this;
			  }

			  /// <summary>
			  /// Add a constraint to this setting. If an error occurs, the constraint should throw <seealso cref="System.ArgumentException"/>.
			  /// Constraints are allowed to modify values and they are applied in the order they are attached to the builder.
			  /// </summary>
			  /// <param name="constraint"> to add. </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public SettingBuilder<T> constraint(@Nonnull final System.Func<T, System.Func<String,String>,T> constraint)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			  public SettingBuilder<T> Constraint( System.Func<T, System.Func<string, string>, T> constraint )
			  {
					if ( ValueConstraints == null )
					{
						 ValueConstraints = new LinkedList<BiFunction<T, Function<string, string>, T>>(); // Must guarantee order
					}
					ValueConstraints.Add( constraint );
					return this;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public org.neo4j.graphdb.config.Setting<T> build()
			  public Setting<T> Build()
			  {
					System.Func<string, System.Func<string, string>, string> valueLookup = Named();
					System.Func<string, System.Func<string, string>, string> defaultLookup = DetermineDefaultLookup( DefaultValue, valueLookup );
					if ( InheritedSetting != null )
					{
						 valueLookup = InheritedValue( valueLookup, InheritedSetting );
						 defaultLookup = InheritedDefault( defaultLookup, InheritedSetting );
					}

					return new DefaultSetting<T>( Name, Parser, valueLookup, defaultLookup, ValueConstraints );
			  }
		 }

		 /// <summary>
		 /// Constructs a <seealso cref="Setting"/> with a specified default value.
		 /// </summary>
		 /// <param name="name"> of the setting, e.g. "dbms.transaction.timeout". </param>
		 /// <param name="parser"> that will convert the string representation to the concrete type T. </param>
		 /// <param name="defaultValue"> the string representation of the default value. </param>
		 /// @param <T> the concrete type of the setting. </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public static <T> org.neo4j.graphdb.config.Setting<T> setting(@Nonnull final String name, @Nonnull final System.Func<String,T> parser, @Nullable final String defaultValue)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public static Setting<T> Setting<T>( string name, System.Func<string, T> parser, string defaultValue )
		 {
			  return ( new SettingBuilder<T>( name, parser, defaultValue ) ).Build();
		 }

		 /// <summary>
		 /// Start building a setting with default value set to <seealso cref="Settings.NO_DEFAULT"/>.
		 /// </summary>
		 /// <param name="name"> of the setting, e.g. "dbms.transaction.timeout". </param>
		 /// <param name="parser"> that will convert the string representation to the concrete type T. </param>
		 /// @param <T> the concrete type of the setting. </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public static <T> SettingBuilder<T> buildSetting(@Nonnull final String name, @Nonnull final System.Func<String, T> parser)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public static SettingBuilder<T> BuildSetting<T>( string name, System.Func<string, T> parser )
		 {
			  return BuildSetting( name, parser, NO_DEFAULT );
		 }

		 /// <summary>
		 /// Start building a setting with a specified default value.
		 /// </summary>
		 /// <param name="name"> of the setting, e.g. "dbms.transaction.timeout". </param>
		 /// <param name="parser"> that will convert the string representation to the concrete type T. </param>
		 /// <param name="defaultValue"> the string representation of the default value. </param>
		 /// @param <T> the concrete type of the setting. </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public static <T> SettingBuilder<T> buildSetting(@Nonnull final String name, @Nonnull final System.Func<String,T> parser, @Nullable final String defaultValue)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public static SettingBuilder<T> BuildSetting<T>( string name, System.Func<string, T> parser, string defaultValue )
		 {
			  return new SettingBuilder<T>( name, parser, defaultValue );
		 }

		 public static System.Func<string, System.Func<string, string>, string> DetermineDefaultLookup( string defaultValue, System.Func<string, System.Func<string, string>, string> valueLookup )
		 {
			  System.Func<string, System.Func<string, string>, string> defaultLookup;
			  if ( !string.ReferenceEquals( defaultValue, null ) )
			  {
					defaultLookup = WithDefault( defaultValue, valueLookup );
			  }
			  else
			  {
					defaultLookup = ( n, from ) => null;
			  }
			  return defaultLookup;
		 }

		 public static Setting<OUT> DerivedSetting<OUT, IN1, IN2>( string name, Setting<IN1> in1, Setting<IN2> in2, System.Func<IN1, IN2, OUT> derivation, System.Func<string, OUT> overrideConverter )
		 {
			  // NOTE:
			  // we do not scope the input settings here (indeed they might be shared...)
			  // if needed we can add a configuration option to allow for it
			  return new ScopeAwareSettingAnonymousInnerClass( name, in1, in2, derivation, overrideConverter );
		 }

		 private class ScopeAwareSettingAnonymousInnerClass : ScopeAwareSetting<OUT>
		 {
			 private string _name;
			 private Setting<IN1> _in1;
			 private Setting<IN2> _in2;
			 private System.Func<IN1, IN2, OUT> _derivation;
			 private System.Func<string, OUT> _overrideConverter;

			 public ScopeAwareSettingAnonymousInnerClass( string name, Setting<IN1> in1, Setting<IN2> in2, System.Func<IN1, IN2, OUT> derivation, System.Func<string, OUT> overrideConverter )
			 {
				 this._name = name;
				 this._in1 = in1;
				 this._in2 = in2;
				 this._derivation = derivation;
				 this._overrideConverter = overrideConverter;
			 }

			 protected internal override string provideName()
			 {
				  return _name;
			 }

			 public override string DefaultValue
			 {
				 get
				 {
					  return NO_DEFAULT;
				 }
			 }

			 public override OUT from( Configuration config )
			 {
				  return config.Get( this );
			 }

			 public override OUT apply( System.Func<string, string> config )
			 {
				  string @override = config( name() );
				  if ( !string.ReferenceEquals( @override, null ) )
				  {
						// Derived settings are intended not to be overridden and we should throw an exception here. However
						// we temporarily need to allow the Desktop app to override the value of the derived setting
						// unsupported.dbms.directories.database because we are not yet in a position to rework it to
						// conform to the standard directory structure layout.
						return _overrideConverter( @override );
				  }
				  return _derivation( _in1.apply( config ), _in2.apply( config ) );
			 }

			 public override string valueDescription()
			 {
				  return _in1.valueDescription();
			 }
		 }

		 public static Setting<OUT> DerivedSetting<OUT, IN1>( string name, Setting<IN1> in1, System.Func<IN1, OUT> derivation, System.Func<string, OUT> overrideConverter )
		 {
			  return new ScopeAwareSettingAnonymousInnerClass2( name, in1, derivation, overrideConverter );
		 }

		 private class ScopeAwareSettingAnonymousInnerClass2 : ScopeAwareSetting<OUT>
		 {
			 private string _name;
			 private Setting<IN1> _in1;
			 private System.Func<IN1, OUT> _derivation;
			 private System.Func<string, OUT> _overrideConverter;

			 public ScopeAwareSettingAnonymousInnerClass2( string name, Setting<IN1> in1, System.Func<IN1, OUT> derivation, System.Func<string, OUT> overrideConverter )
			 {
				 this._name = name;
				 this._in1 = in1;
				 this._derivation = derivation;
				 this._overrideConverter = overrideConverter;
			 }

			 protected internal override string provideName()
			 {
				  return _name;
			 }

			 public override string DefaultValue
			 {
				 get
				 {
					  return NO_DEFAULT;
				 }
			 }

			 public override OUT from( Configuration config )
			 {
				  return config.Get( this );
			 }

			 public override OUT apply( System.Func<string, string> config )
			 {
				  string @override = config( name() );
				  if ( !string.ReferenceEquals( @override, null ) )
				  {
						return _overrideConverter( @override );
				  }
				  return _derivation( _in1.apply( config ) );
			 }

			 public override string valueDescription()
			 {
				  return _in1.valueDescription();
			 }
		 }

		 public static Setting<File> PathSetting( string name, string defaultValue )
		 {
			  return new FileSetting( name, defaultValue );
		 }

		 public static Setting<File> PathSetting( string name, string defaultValue, Setting<File> relativeRoot )
		 {
			  return new FileSetting( name, defaultValue, relativeRoot );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static <T> System.Func<String,System.Func<String, String>, String> inheritedValue(final System.Func<String,System.Func<String,String>, String> lookup, final org.neo4j.graphdb.config.Setting<T> inheritedSetting)
		 private static System.Func<string, System.Func<string, string>, string> InheritedValue<T>( System.Func<string, System.Func<string, string>, string> lookup, Setting<T> inheritedSetting )
		 {
			  return ( name, settings ) =>
			  {
				string value = lookup( name, settings );
				if ( string.ReferenceEquals( value, null ) )
				{
					 value = ( ( SettingHelper<T> ) inheritedSetting ).Lookup( settings );
				}
				return value;
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static <T> System.Func<String,System.Func<String, String>, String> inheritedDefault(final System.Func<String,System.Func<String,String>, String> lookup, final org.neo4j.graphdb.config.Setting<T> inheritedSetting)
		 private static System.Func<string, System.Func<string, string>, string> InheritedDefault<T>( System.Func<string, System.Func<string, string>, string> lookup, Setting<T> inheritedSetting )
		 {
			  return ( name, settings ) =>
			  {
				string value = lookup( name, settings );
				if ( string.ReferenceEquals( value, null ) )
				{
					 value = ( ( SettingHelper<T> ) inheritedSetting ).DefaultLookup( settings );
				}
				return value;
			  };
		 }

		 public static readonly System.Func<string, int> INTEGER = new FuncAnonymousInnerClass();

		 private class FuncAnonymousInnerClass : System.Func<string, int>
		 {
			 public override int? apply( string value )
			 {
				  try
				  {
						return Convert.ToInt32( value );
				  }
				  catch ( System.FormatException )
				  {
						throw new System.ArgumentException( "not a valid integer value" );
				  }
			 }

			 public override string ToString()
			 {
				  return "an integer";
			 }
		 }

		 public static readonly System.Func<string, long> LONG = new FuncAnonymousInnerClass2();

		 private class FuncAnonymousInnerClass2 : System.Func<string, long>
		 {
			 public override long? apply( string value )
			 {
				  try
				  {
						return Convert.ToInt64( value );
				  }
				  catch ( System.FormatException )
				  {
						throw new System.ArgumentException( "not a valid long value" );
				  }
			 }

			 public override string ToString()
			 {
				  return "a long";
			 }
		 }

		 public static readonly System.Func<string, bool> BOOLEAN = new FuncAnonymousInnerClass3();

		 private class FuncAnonymousInnerClass3 : System.Func<string, bool>
		 {
			 public override bool? apply( string value )
			 {
				  if ( value.Equals( "true", StringComparison.OrdinalIgnoreCase ) )
				  {
						return true;
				  }
				  else if ( value.Equals( "false", StringComparison.OrdinalIgnoreCase ) )
				  {
						return false;
				  }
				  else
				  {
						throw new System.ArgumentException( "must be 'true' or 'false'" );
				  }
			 }

			 public override string ToString()
			 {
				  return "a boolean";
			 }
		 }

		 public static readonly System.Func<string, float> FLOAT = new FuncAnonymousInnerClass4();

		 private class FuncAnonymousInnerClass4 : System.Func<string, float>
		 {
			 public override float? apply( string value )
			 {
				  try
				  {
						return Convert.ToSingle( value );
				  }
				  catch ( System.FormatException )
				  {
						throw new System.ArgumentException( "not a valid float value" );
				  }
			 }

			 public override string ToString()
			 {
				  return "a float";
			 }
		 }

		 public static readonly System.Func<string, double> DOUBLE = new FuncAnonymousInnerClass5();

		 private class FuncAnonymousInnerClass5 : System.Func<string, double>
		 {
			 public override double? apply( string value )
			 {
				  try
				  {
						return Convert.ToDouble( value );
				  }
				  catch ( System.FormatException )
				  {
						throw new System.ArgumentException( "not a valid double value" );
				  }
			 }

			 public override string ToString()
			 {
				  return "a double";
			 }
		 }

		 public static readonly System.Func<string, string> STRING = new FuncAnonymousInnerClass6();

		 private class FuncAnonymousInnerClass6 : System.Func<string, string>
		 {
			 public override string apply( string value )
			 {
				  return value.Trim();
			 }

			 public override string ToString()
			 {
				  return "a string";
			 }
		 }

		 public static readonly System.Func<string, IList<string>> StringList = List( SEPARATOR, STRING );

		 public static readonly System.Func<string, HostnamePort> HOSTNAME_PORT = new FuncAnonymousInnerClass7();

		 private class FuncAnonymousInnerClass7 : System.Func<string, HostnamePort>
		 {
			 public override HostnamePort apply( string value )
			 {
				  return new HostnamePort( value );
			 }

			 public override string ToString()
			 {
				  return "a hostname and port";
			 }
		 }

		 public static readonly System.Func<string, Duration> DURATION = new FuncAnonymousInnerClass8();

		 private class FuncAnonymousInnerClass8 : System.Func<string, Duration>
		 {
			 public override Duration apply( string value )
			 {
				  return Duration.ofMillis( TimeUtil.parseTimeMillis.apply( value ) );
			 }

			 public override string ToString()
			 {
				  return "a duration (" + TimeUtil.VALID_TIME_DESCRIPTION + ")";
			 }
		 }

		 public static readonly System.Func<string, ZoneId> TIMEZONE = new FuncAnonymousInnerClass9();

		 private class FuncAnonymousInnerClass9 : System.Func<string, ZoneId>
		 {
			 public override ZoneId apply( string value )
			 {
				  return DateTimeValue.parseZoneOffsetOrZoneName( value );
			 }

			 public override string ToString()
			 {
				  return "a string describing a timezone, either described by offset (e.g. '+02:00') or by name (e.g. 'Europe/Stockholm')";
			 }
		 }

		 public static readonly System.Func<string, ListenSocketAddress> LISTEN_SOCKET_ADDRESS = new FuncAnonymousInnerClass10();

		 private class FuncAnonymousInnerClass10 : System.Func<string, ListenSocketAddress>
		 {
			 public override ListenSocketAddress apply( string value )
			 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
				  return SocketAddressParser.socketAddress( value, ListenSocketAddress::new );
			 }

			 public override string ToString()
			 {
				  return "a listen socket address";
			 }
		 }

		 public static readonly System.Func<string, AdvertisedSocketAddress> ADVERTISED_SOCKET_ADDRESS = new FuncAnonymousInnerClass11();

		 private class FuncAnonymousInnerClass11 : System.Func<string, AdvertisedSocketAddress>
		 {
			 public override AdvertisedSocketAddress apply( string value )
			 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
				  return SocketAddressParser.socketAddress( value, AdvertisedSocketAddress::new );
			 }

			 public override string ToString()
			 {
				  return "an advertised socket address";
			 }
		 }

		 public static BaseSetting<ListenSocketAddress> ListenAddress( string name, int defaultPort )
		 {
			  return new ScopeAwareSettingAnonymousInnerClass3( name, defaultPort );
		 }

		 private class ScopeAwareSettingAnonymousInnerClass3 : ScopeAwareSetting<ListenSocketAddress>
		 {
			 private string _name;
			 private int _defaultPort;

			 public ScopeAwareSettingAnonymousInnerClass3( string name, int defaultPort )
			 {
				 this._name = name;
				 this._defaultPort = defaultPort;
			 }

			 protected internal override string provideName()
			 {
				  return _name;
			 }

			 public override string DefaultValue
			 {
				 get
				 {
					  return default_listen_address.DefaultValue + ":" + _defaultPort;
				 }
			 }

			 public override ListenSocketAddress from( Configuration config )
			 {
				  return config.Get( this );
			 }

			 public override ListenSocketAddress apply( System.Func<string, string> config )
			 {
				  string name = name();
				  string value = config( name );
				  string hostname = default_listen_address.apply( config );

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
				  return SocketAddressParser.deriveSocketAddress( name, value, hostname, _defaultPort, ListenSocketAddress::new );
			 }

			 public override string valueDescription()
			 {
				  return LISTEN_SOCKET_ADDRESS.ToString();
			 }
		 }

		 public static BaseSetting<AdvertisedSocketAddress> AdvertisedAddress( string name, Setting<ListenSocketAddress> listenAddressSetting )
		 {
			  return new ScopeAwareSettingAnonymousInnerClass4( name, listenAddressSetting );
		 }

		 private class ScopeAwareSettingAnonymousInnerClass4 : ScopeAwareSetting<AdvertisedSocketAddress>
		 {
			 private string _name;
			 private Setting<ListenSocketAddress> _listenAddressSetting;

			 public ScopeAwareSettingAnonymousInnerClass4( string name, Setting<ListenSocketAddress> listenAddressSetting )
			 {
				 this._name = name;
				 this._listenAddressSetting = listenAddressSetting;
			 }

			 protected internal override string provideName()
			 {
				  return _name;
			 }

			 public override string DefaultValue
			 {
				 get
				 {
					  return default_advertised_address.DefaultValue + ":" + LISTEN_SOCKET_ADDRESS.apply( _listenAddressSetting.DefaultValue ).socketAddress().Port;
				 }
			 }

			 public override AdvertisedSocketAddress from( Configuration config )
			 {
				  return config.Get( this );
			 }

			 public override AdvertisedSocketAddress apply( System.Func<string, string> config )
			 {
				  ListenSocketAddress listenSocketAddress = _listenAddressSetting.apply( config );
				  string hostname = default_advertised_address.apply( config );
				  int port = listenSocketAddress.SocketAddressConflict().Port;

				  string name = name();
				  string value = config( name );

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
				  return SocketAddressParser.deriveSocketAddress( name, value, hostname, port, AdvertisedSocketAddress::new );
			 }

			 public override void withScope( System.Func<string, string> scopingRule )
			 {
				  base.withScope( scopingRule );
				  _listenAddressSetting.withScope( scopingRule );
			 }

			 public override string valueDescription()
			 {
				  return ADVERTISED_SOCKET_ADDRESS.ToString();
			 }
		 }

		 public static readonly System.Func<string, long> BYTES = new FuncAnonymousInnerClass12();

		 private class FuncAnonymousInnerClass12 : System.Func<string, long>
		 {
			 public override long? apply( string value )
			 {
				  long bytes;
				  try
				  {
						bytes = ByteUnit.parse( value );
				  }
				  catch ( System.ArgumentException )
				  {
						throw new System.ArgumentException( format( "%s is not a valid size, must be e.g. 10, 5K, 1M, 11G", value ) );
				  }
				  if ( bytes < 0 )
				  {
						throw new System.ArgumentException( value + " is not a valid number of bytes. Must be positive or zero." );
				  }
				  return bytes;
			 }

			 public override string ToString()
			 {
				  return "a byte size (valid multipliers are `" + _sizeUnits.Replace( ", ", "`, `" ) + "`)";
			 }
		 }

		 public static readonly System.Func<string, URI> URI = new FuncAnonymousInnerClass13();

		 private class FuncAnonymousInnerClass13 : System.Func<string, URI>
		 {
			 public override URI apply( string value )
			 {
				  try
				  {
						return new URI( value );
				  }
				  catch ( URISyntaxException )
				  {
						throw new System.ArgumentException( "not a valid URI" );
				  }
			 }

			 public override string ToString()
			 {
				  return "a URI";
			 }
		 }

		 public static readonly System.Func<string, URI> NORMALIZED_RELATIVE_URI = new FuncAnonymousInnerClass14();

		 private class FuncAnonymousInnerClass14 : System.Func<string, URI>
		 {
			 public override URI apply( string value )
			 {
				  try
				  {
						string normalizedUri = ( new URI( value ) ).normalize().Path;
						if ( normalizedUri.EndsWith( "/", StringComparison.Ordinal ) )
						{
							 // Force the string end without "/"
							 normalizedUri = normalizedUri.Substring( 0, normalizedUri.Length - 1 );
						}
						return new URI( normalizedUri );
				  }
				  catch ( URISyntaxException )
				  {
						throw new System.ArgumentException( "not a valid URI" );
				  }
			 }

			 public override string ToString()
			 {
				  return "a URI";
			 }
		 }

		 public static readonly System.Func<string, File> PATH = new FuncAnonymousInnerClass15();

		 private class FuncAnonymousInnerClass15 : System.Func<string, File>
		 {
			 public override File apply( string setting )
			 {
				  File file = new File( fixSeparatorsInPath( setting ) );
				  if ( !file.Absolute )
				  {
						throw new System.ArgumentException( "Paths must be absolute. Got " + file );
				  }
				  return file;
			 }

			 public override string ToString()
			 {
				  return "a path";
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T extends Enum<T>> System.Func<String, T> optionsObeyCase(final Class<T> enumClass)
		 public static System.Func<string, T> OptionsObeyCase<T>( Type enumClass ) where T : Enum<T>
		 {
				 enumClass = typeof( T );
			  return Options( EnumSet.allOf( enumClass ), false );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T extends Enum<T>> System.Func<String, T> optionsIgnoreCase(final Class<T> enumClass)
		 public static System.Func<string, T> OptionsIgnoreCase<T>( Type enumClass ) where T : Enum<T>
		 {
				 enumClass = typeof( T );
			  return Options( EnumSet.allOf( enumClass ), true );
		 }

		 public static System.Func<string, T> OptionsObeyCase<T>( params T[] optionValues )
		 {
			  return Options( Iterables.iterable( optionValues ), false );
		 }

		 public static System.Func<string, T> OptionsIgnoreCase<T>( params T[] optionValues )
		 {
			  return Options( Iterables.iterable( optionValues ), true );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> System.Func<String, T> options(final Iterable<T> optionValues, final boolean ignoreCase)
		 public static System.Func<string, T> Options<T>( IEnumerable<T> optionValues, bool ignoreCase )
		 {
			  return new FuncAnonymousInnerClass16( optionValues, ignoreCase );
		 }

		 private class FuncAnonymousInnerClass16 : System.Func<string, T>
		 {
			 private IEnumerable<T> _optionValues;
			 private bool _ignoreCase;

			 public FuncAnonymousInnerClass16( IEnumerable<T> optionValues, bool ignoreCase )
			 {
				 this._optionValues = optionValues;
				 this._ignoreCase = ignoreCase;
			 }

			 public override T apply( string value )
			 {
				  foreach ( T optionValue in _optionValues )
				  {
						string allowedValue = optionValue.ToString();

						if ( allowedValue.Equals( value ) || ( _ignoreCase && allowedValue.Equals( value, StringComparison.OrdinalIgnoreCase ) ) )
						{
							 return optionValue;
						}
				  }
				  string possibleValues = Iterables.asList( _optionValues ).ToString();
				  throw new System.ArgumentException( "must be one of " + possibleValues + " case " + ( _ignoreCase ? "insensitive" : "sensitive" ) );
			 }

			 public override string ToString()
			 {
				  return DescribeOneOf( _optionValues );
			 }
		 }

		 /// 
		 /// <param name="optionValues"> iterable of objects with descriptive toString methods </param>
		 /// <returns> a string describing possible values like "one of `X, Y, Z`" </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public static String describeOneOf(@Nonnull Iterable optionValues)
		 public static string DescribeOneOf( System.Collections.IEnumerable optionValues )
		 {
			  StringBuilder builder = new StringBuilder();
			  builder.Append( "one of `" );
			  string comma = "";
			  foreach ( object optionValue in optionValues )
			  {
					builder.Append( comma ).Append( optionValue );
					comma = "`, `";
			  }
			  builder.Append( '`' );
			  return builder.ToString();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> System.Func<String, java.util.List<T>> list(final String separator, final System.Func<String, T> itemParser)
		 public static System.Func<string, IList<T>> List<T>( string separator, System.Func<string, T> itemParser )
		 {
			  return new FuncAnonymousInnerClass17( separator, itemParser );
		 }

		 private class FuncAnonymousInnerClass17 : System.Func<string, IList<T>>
		 {
			 private string _separator;
			 private System.Func<string, T> _itemParser;

			 public FuncAnonymousInnerClass17( string separator, System.Func<string, T> itemParser )
			 {
				 this._separator = separator;
				 this._itemParser = itemParser;
			 }

			 public override IList<T> apply( string value )
			 {
				  IList<T> list = new List<T>();
				  string[] parts = value.Split( _separator, true );
				  foreach ( string part in parts )
				  {
						part = part.Trim();
						if ( StringUtils.isNotEmpty( part ) )
						{
							 list.Add( _itemParser( part ) );
						}
				  }
				  return list;
			 }

			 public override string ToString()
			 {
				  return "a list separated by \"" + _separator + "\" where items are " + _itemParser;
			 }
		 }

		 // Modifiers
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Func<String, System.Func<String, String>, String> matches(final String regex)
		 public static System.Func<string, System.Func<string, string>, string> Matches( string regex )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.regex.Pattern pattern = java.util.regex.Pattern.compile(regex);
			  Pattern pattern = Pattern.compile( regex );

			  return new FuncAnonymousInnerClass18( regex, pattern );
		 }

		 private class FuncAnonymousInnerClass18 : System.Func<string, System.Func<string, string>, string>
		 {
			 private string _regex;
			 private Pattern _pattern;

			 public FuncAnonymousInnerClass18( string regex, Pattern pattern )
			 {
				 this._regex = regex;
				 this._pattern = pattern;
			 }

			 public override string apply( string value, System.Func<string, string> settings )
			 {
				  if ( !_pattern.matcher( value ).matches() )
				  {
						throw new System.ArgumentException( "value does not match expression:" + _regex );
				  }

				  return value;
			 }

			 public override string ToString()
			 {
				  return format( MATCHES_PATTERN_MESSAGE, _regex );
			 }
		 }

		 public static readonly System.Func<IList<string>, System.Func<string, string>, IList<string>> nonEmptyList = new FuncAnonymousInnerClass19();

		 private class FuncAnonymousInnerClass19 : System.Func<IList<string>, System.Func<string, string>, IList<string>>
		 {
			 public override IList<string> apply( IList<string> values, System.Func<string, string> settings )
			 {
				  if ( values.Count == 0 )
				  {
						throw new System.ArgumentException( "setting must not be empty" );
				  }
				  return values;
			 }

			 public override string ToString()
			 {
				  return "non-empty list";
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Func<java.util.List<String>,System.Func<String,String>,java.util.List<String>> matchesAny(final String regex)
		 public static System.Func<IList<string>, System.Func<string, string>, IList<string>> MatchesAny( string regex )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.regex.Pattern pattern = java.util.regex.Pattern.compile(regex);
			  Pattern pattern = Pattern.compile( regex );

			  return new FuncAnonymousInnerClass20( regex, pattern );
		 }

		 private class FuncAnonymousInnerClass20 : System.Func<IList<string>, System.Func<string, string>, IList<string>>
		 {
			 private string _regex;
			 private Pattern _pattern;

			 public FuncAnonymousInnerClass20( string regex, Pattern pattern )
			 {
				 this._regex = regex;
				 this._pattern = pattern;
			 }

			 public override IList<string> apply( IList<string> values, System.Func<string, string> settings )
			 {
				  foreach ( string value in values )
				  {
						if ( !_pattern.matcher( value ).matches() )
						{
							 throw new System.ArgumentException( "value does not match expression:" + _regex );
						}
				  }

				  return values;
			 }

			 public override string ToString()
			 {
				  return format( MATCHES_PATTERN_MESSAGE, _regex );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Func<String,System.Func<String,String>,String> except(final String... forbiddenValues)
		 public static System.Func<string, System.Func<string, string>, string> Except( params string[] forbiddenValues )
		 {
			  return new FuncAnonymousInnerClass21( forbiddenValues );
		 }

		 private class FuncAnonymousInnerClass21 : System.Func<string, System.Func<string, string>, string>
		 {
			 private string[] _forbiddenValues;

			 public FuncAnonymousInnerClass21( string[] forbiddenValues )
			 {
				 this._forbiddenValues = forbiddenValues;
			 }

			 public override string apply( string value, System.Func<string, string> stringStringFunction )
			 {
				  if ( StringUtils.isNotBlank( value ) )
				  {
						if ( ArrayUtils.contains( _forbiddenValues, value ) )
						{
							 throw new System.ArgumentException( format( "not allowed value is: %s", value ) );
						}
				  }
				  return value;
			 }

			 public override string ToString()
			 {
				  if ( _forbiddenValues.Length > 1 )
				  {
						return format( "is none of %s", Arrays.ToString( _forbiddenValues ) );
				  }
				  else if ( _forbiddenValues.Length == 1 )
				  {
						return format( "is not `%s`", _forbiddenValues[0] );
				  }
				  return "";
			 }
		 }

		 public static System.Func<long, System.Func<string, string>, long> PowerOf2()
		 {
			  return new FuncAnonymousInnerClass22();
		 }

		 private class FuncAnonymousInnerClass22 : System.Func<long, System.Func<string, string>, long>
		 {
			 public override long? apply( long? value, System.Func<string, string> settings )
			 {
				  if ( value != null && !Numbers.isPowerOfTwo( value.Value ) )
				  {
						throw new System.ArgumentException( "only power of 2 values allowed" );
				  }
				  return value;
			 }

			 public override string ToString()
			 {
				  return "is power of 2";
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T extends Comparable<T>> System.Func<T, System.Func<String, String>, T> min(final T min)
		 public static System.Func<T, System.Func<string, string>, T> Min<T>( T min ) where T : IComparable<T>
		 {
			  return new FuncAnonymousInnerClass23( min );
		 }

		 private class FuncAnonymousInnerClass23 : System.Func<T, System.Func<string, string>, T>
		 {
			 private T _min;

			 public FuncAnonymousInnerClass23( T min )
			 {
				 this._min = min;
			 }

			 public override T apply( T value, System.Func<string, string> settings )
			 {
				  if ( value != null && value.compareTo( _min ) < 0 )
				  {
						throw new System.ArgumentException( format( "minimum allowed value is: %s", _min ) );
				  }
				  return value;
			 }

			 public override string ToString()
			 {
				  return "is minimum `" + _min + "`";
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T extends Comparable<T>> System.Func<T, System.Func<String, String>, T> max(final T max)
		 public static System.Func<T, System.Func<string, string>, T> Max<T>( T max ) where T : IComparable<T>
		 {
			  return new FuncAnonymousInnerClass24( max );
		 }

		 private class FuncAnonymousInnerClass24 : System.Func<T, System.Func<string, string>, T>
		 {
			 private T _max;

			 public FuncAnonymousInnerClass24( T max )
			 {
				 this._max = max;
			 }

			 public override T apply( T value, System.Func<string, string> settings )
			 {
				  if ( value != null && value.compareTo( _max ) > 0 )
				  {
						throw new System.ArgumentException( format( "maximum allowed value is: %s", _max ) );
				  }
				  return value;
			 }

			 public override string ToString()
			 {
				  return "is maximum `" + _max + "`";
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T extends Comparable<T>> System.Func<T, System.Func<String, String>, T> range(final T min, final T max)
		 public static System.Func<T, System.Func<string, string>, T> Range<T>( T min, T max ) where T : IComparable<T>
		 {
			  return new FuncAnonymousInnerClass25( min, max );
		 }

		 private class FuncAnonymousInnerClass25 : System.Func<T, System.Func<string, string>, T>
		 {
			 private T _min;
			 private T _max;

			 public FuncAnonymousInnerClass25( T min, T max )
			 {
				 this._min = min;
				 this._max = max;
			 }

			 public override T apply( T from1, System.Func<string, string> from2 )
			 {
				  return _min( _min ).apply( _max( _max ).apply( from1, from2 ), from2 );
			 }

			 public override string ToString()
			 {
				  return format( "is in the range `%s` to `%s`", _min, _max );
			 }
		 }

		 public static readonly System.Func<int, System.Func<string, string>, int> Port = IllegalValueMessage( "must be a valid port number", Range( 0, 65535 ) );

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> System.Func<T, System.Func<String, String>, T> illegalValueMessage(final String message, final System.Func<T,System.Func<String,String>,T> valueFunction)
		 public static System.Func<T, System.Func<string, string>, T> IllegalValueMessage<T>( string message, System.Func<T, System.Func<string, string>, T> valueFunction )
		 {
			  return new FuncAnonymousInnerClass26( message, valueFunction );
		 }

		 private class FuncAnonymousInnerClass26 : System.Func<T, System.Func<string, string>, T>
		 {
			 private string _message;
			 private System.Func<T, System.Func<string, string>, T> _valueFunction;

			 public FuncAnonymousInnerClass26( string message, System.Func<T, System.Func<string, string>, T> valueFunction )
			 {
				 this._message = message;
				 this._valueFunction = valueFunction;
			 }

			 public override T apply( T from1, System.Func<string, string> from2 )
			 {
				  try
				  {
						return _valueFunction( from1, from2 );
				  }
				  catch ( System.ArgumentException )
				  {
						throw new System.ArgumentException( _message );
				  }
			 }

			 public override string ToString()
			 {
				  string description = _message;
				  if ( _valueFunction != null && !format( MATCHES_PATTERN_MESSAGE, ANY ).Equals( _valueFunction.ToString() ) )
				  {
						description += " (" + _valueFunction + ")";
				  }
				  return description;
			 }
		 }

		 // Setting converters and constraints
		 public static long ParseLongWithUnit( string numberWithPotentialUnit )
		 {
			  int firstNonDigitIndex = FindFirstNonDigit( numberWithPotentialUnit );
			  string number = numberWithPotentialUnit.Substring( 0, firstNonDigitIndex );

			  long multiplier = 1;
			  if ( firstNonDigitIndex < numberWithPotentialUnit.Length )
			  {
					string unit = numberWithPotentialUnit.Substring( firstNonDigitIndex );
					if ( unit.Equals( "k", StringComparison.OrdinalIgnoreCase ) )
					{
						 multiplier = 1024;
					}
					else if ( unit.Equals( "m", StringComparison.OrdinalIgnoreCase ) )
					{
						 multiplier = 1024 * 1024;
					}
					else if ( unit.Equals( "g", StringComparison.OrdinalIgnoreCase ) )
					{
						 multiplier = 1024 * 1024 * 1024;
					}
					else
					{
						 throw new System.ArgumentException( "Illegal unit '" + unit + "' for number '" + numberWithPotentialUnit + "'" );
					}
			  }

			  return parseLong( number ) * multiplier;
		 }

		 /// <returns> index of first non-digit character in {@code numberWithPotentialUnit}. If all digits then
		 /// {@code numberWithPotentialUnit.length()} is returned. </returns>
		 private static int FindFirstNonDigit( string numberWithPotentialUnit )
		 {
			  int firstNonDigitIndex = numberWithPotentialUnit.Length;
			  for ( int i = 0; i < numberWithPotentialUnit.Length; i++ )
			  {
					if ( !isDigit( numberWithPotentialUnit[i] ) )
					{
						 firstNonDigitIndex = i;
						 break;
					}
			  }
			  return firstNonDigitIndex;
		 }

		 // Setting helpers
		 private static System.Func<string, System.Func<string, string>, string> Named()
		 {
			  return ( name, settings ) => settings.apply( name );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static System.Func<String,System.Func<String,String>,String> withDefault(final String defaultValue, final System.Func<String,System.Func<String,String>,String> lookup)
		 private static System.Func<string, System.Func<string, string>, string> WithDefault( string defaultValue, System.Func<string, System.Func<string, string>, string> lookup )
		 {
			  return ( name, settings ) =>
			  {
				string value = lookup( name, settings );
				if ( string.ReferenceEquals( value, null ) )
				{
					 return defaultValue;
				}
				else
				{
					 return value;
				}
			  };
		 }

		 public static Setting<T> LegacyFallback<T>( Setting<T> fallbackSetting, Setting<T> newSetting )
		 {
			  return new SettingAnonymousInnerClass( fallbackSetting, newSetting );
		 }

		 private class SettingAnonymousInnerClass : Setting<T>
		 {
			 private Setting<T> _fallbackSetting;
			 private Setting<T> _newSetting;

			 public SettingAnonymousInnerClass( Setting<T> fallbackSetting, Setting<T> newSetting )
			 {
				 this._fallbackSetting = fallbackSetting;
				 this._newSetting = newSetting;
			 }

			 public string name()
			 {
				  return _newSetting.name();
			 }

			 public string DefaultValue
			 {
				 get
				 {
					  return _newSetting.DefaultValue;
				 }
			 }

			 public T from( Configuration config )
			 {
				  return _newSetting.from( config );
			 }

			 public T apply( System.Func<string, string> config )
			 {
				  string newValue = config( _newSetting.name() );
				  return string.ReferenceEquals( newValue, null ) ? _fallbackSetting.apply( config ) : _newSetting.apply( config );
			 }

			 public void withScope( System.Func<string, string> scopingRule )
			 {
				  _newSetting.withScope( scopingRule );
			 }

			 public string valueDescription()
			 {
				  return _newSetting.valueDescription();
			 }

			 public Optional<string> description()
			 {
				  return _newSetting.description();
			 }

			 public bool dynamic()
			 {
				  return _newSetting.dynamic();
			 }

			 public bool deprecated()
			 {
				  return _newSetting.deprecated();
			 }

			 public Optional<string> replacement()
			 {
				  return _newSetting.replacement();
			 }

			 public bool @internal()
			 {
				  return _newSetting.@internal();
			 }

			 public bool secret()
			 {
				  return _newSetting.secret();
			 }

			 public Optional<string> documentedDefaultValue()
			 {
				  return _newSetting.documentedDefaultValue();
			 }
		 }

		 private Settings()
		 {
			  throw new AssertionError();
		 }

		 public class DefaultSetting<T> : ScopeAwareSetting<T>, SettingHelper<T>
		 {
			  internal readonly string Name;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly System.Func<string, T> ParserConflict;
			  internal readonly System.Func<string, System.Func<string, string>, string> ValueLookup;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly System.Func<string, System.Func<string, string>, string> DefaultLookupConflict;
			  internal readonly IList<System.Func<T, System.Func<string, string>, T>> ValueConverters;

			  protected internal DefaultSetting( string name, System.Func<string, T> parser, System.Func<string, System.Func<string, string>, string> valueLookup, System.Func<string, System.Func<string, string>, string> defaultLookup, IList<System.Func<T, System.Func<string, string>, T>> valueConverters )
			  {
					this.Name = name;
					this.ParserConflict = parser;
					this.ValueLookup = valueLookup;
					this.DefaultLookupConflict = defaultLookup;
					this.ValueConverters = valueConverters;
			  }

			  protected internal override string ProvideName()
			  {
					return Name;
			  }

			  public override string DefaultValue
			  {
				  get
				  {
						return DefaultLookup( from => null );
				  }
			  }

			  public override T From( Configuration config )
			  {
					return config.Get( this );
			  }

			  public override Optional<System.Func<string, T>> Parser
			  {
				  get
				  {
						return ParserConflict;
				  }
			  }

			  public override string Lookup( System.Func<string, string> settings )
			  {
					return ValueLookup.apply( Name(), settings );
			  }

			  public override string DefaultLookup( System.Func<string, string> settings )
			  {
					return DefaultLookupConflict.apply( Name(), settings );
			  }

			  public override T Apply( System.Func<string, string> settings )
			  {
					// Lookup value as string
					string value = Lookup( settings );

					// Try defaults
					if ( string.ReferenceEquals( value, null ) )
					{
						 try
						 {
							  value = DefaultLookup( settings );
						 }
						 catch ( Exception )
						 {
							  throw new System.ArgumentException( format( "Missing mandatory setting '%s'", Name() ) );
						 }
					}

					// If still null, return null
					if ( string.ReferenceEquals( value, null ) )
					{
						 return default( T );
					}

					// Parse value
					T result;
					try
					{
						 result = ParserConflict.apply( value );
						 // Apply converters and constraints
						 if ( ValueConverters != null )
						 {
							  foreach ( System.Func<T, System.Func<string, string>, T> valueConverter in ValueConverters )
							  {
									result = valueConverter( result, settings );
							  }
						 }
					}
					catch ( System.ArgumentException e )
					{
						 throw new InvalidSettingException( Name(), value, e.Message );
					}

					return result;
			  }

			  public override string ValueDescription()
			  {
					StringBuilder builder = new StringBuilder();
					builder.Append( Name() ).Append(" is ").Append(ParserConflict);

					if ( ValueConverters != null && ValueConverters.Count > 0 )
					{
						 builder.Append( " which " );
						 bool first = true;
						 foreach ( System.Func<T, System.Func<string, string>, T> valueConverter in ValueConverters )
						 {
							  if ( !first )
							  {
									builder.Append( ", and " );
							  }
							  builder.Append( valueConverter );
							  first = false;
						 }
					}

					return builder.ToString();
			  }
		 }

		 private class FileSetting : ScopeAwareSetting<File>
		 {
			  internal readonly string Name;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string DefaultValueConflict;
			  internal readonly Setting<File> RelativeRoot;

			  internal FileSetting( string name, string defaultValue ) : this( name, defaultValue, GraphDatabaseSettings.neo4j_home )
			  {
			  }

			  internal FileSetting( string name, string defaultValue, Setting<File> relativeRoot )
			  {
					this.Name = name;
					this.DefaultValueConflict = defaultValue;
					this.RelativeRoot = relativeRoot;
			  }

			  protected internal override string ProvideName()
			  {
					return Name;
			  }

			  public override string DefaultValue
			  {
				  get
				  {
						return DefaultValueConflict;
				  }
			  }

			  public override File From( Configuration config )
			  {
					return config.Get( this );
			  }

			  public override File Apply( System.Func<string, string> config )
			  {
					string value = config( Name() );
					if ( string.ReferenceEquals( value, null ) )
					{
						 value = DefaultValueConflict;
					}
					if ( string.ReferenceEquals( value, null ) )
					{
						 return null;
					}

					string setting = fixSeparatorsInPath( value );
					File settingFile = new File( setting );

					if ( settingFile.Absolute )
					{
						 return settingFile;
					}
					else
					{
						 return new File( RelativeRoot.apply( config ), setting );
					}
			  }

			  public override string ValueDescription()
			  {
					return "A filesystem path; relative paths are resolved against the root, _<" + RelativeRoot.name() + ">_";
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.neo4j.graphdb.config.BaseSetting<String> prefixSetting(final String name, final System.Func<String,String> parser, final String defaultValue)
		 public static BaseSetting<string> PrefixSetting( string name, System.Func<string, string> parser, string defaultValue )
		 {
			  System.Func<string, System.Func<string, string>, string> valueLookup = ( n, settings ) => settings.apply( n );
			  System.Func<string, System.Func<string, string>, string> defaultLookup = DetermineDefaultLookup( defaultValue, valueLookup );

			  return new DefaultSettingAnonymousInnerClass( name, parser, valueLookup, defaultLookup, Collections.emptyList() );
		 }

		 private class DefaultSettingAnonymousInnerClass : Settings.DefaultSetting<string>
		 {
			 public DefaultSettingAnonymousInnerClass( string name, System.Func<string, string> parser, System.Func<string, System.Func<string, string>, string> valueLookup, System.Func<string, System.Func<string, string>, string> defaultLookup, UnknownType emptyList ) : base( name, parser, valueLookup, defaultLookup, emptyList )
			 {
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Map<String,String> validate(java.util.Map<String,String> rawConfig, System.Action<String> warningConsumer) throws org.neo4j.graphdb.config.InvalidSettingException
			 public override IDictionary<string, string> validate( IDictionary<string, string> rawConfig, System.Action<string> warningConsumer )
			 {
				  // Validate setting, if present or default value otherwise
				  try
				  {
						apply( rawConfig.get );
						// only return if it was present though

//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
						return rawConfig.SetOfKeyValuePairs().Where(entry => entry.Key.StartsWith(name())).collect(CollectorsUtil.entriesToMap());
				  }
				  catch ( Exception e )
				  {
						throw new InvalidSettingException( e.Message, e );
				  }
			 }
		 }
	}

}