using System;
using System.Collections.Generic;
using System.Diagnostics;
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
namespace Org.Neo4j.Helpers
{

	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Org.Neo4j.Kernel.impl.util;

	/// <summary>
	/// Parses a String[] argument from a main-method. It expects values to be either
	/// key/value pairs or just "orphan" values (w/o a key associated).
	/// <para>
	/// A key is defined with one or more dashes in the beginning, for example:
	/// 
	/// <pre>
	///   '-path'
	///   '--path'
	/// </pre>
	/// 
	/// A key/value pair can be either one single String from the array where there's
	/// a '=' delimiter between the key and value, like so:
	/// 
	/// <pre>
	///   '--path=/my/path/to/something'
	/// </pre>
	/// ...or consist of two (consecutive) strings from the array, like so:
	/// <pre>
	///   '-path' '/my/path/to/something'
	/// </pre>
	/// 
	/// Multiple values for an option is supported, however the only means of extracting all values is be
	/// using <seealso cref="interpretOptions(string, Function, Function, Validator...)"/>, all other methods revolve
	/// around single value, i.e. will fail if there are multiple.
	/// 
	/// Options can have metadata which can be extracted using
	/// <seealso cref="interpretOptions(string, Function, Function, Validator...)"/>. Metadata looks like:
	/// <pre>
	///   --my-option:Metadata my-value
	/// </pre>
	/// 
	/// where {@code Metadata} would be the metadata of {@code my-value}.
	/// </para>
	/// </summary>
	[Obsolete]
	public class Args
	{
		 private const char OPTION_METADATA_DELIMITER = ':';

		 [Obsolete]
		 public class ArgsParser
		 {
			  internal readonly string[] Flags;

			  internal ArgsParser( params string[] flags )
			  {
					this.Flags = Objects.requireNonNull( flags );
			  }

			  public virtual Args Parse( params string[] arguments )
			  {
					return new Args( Flags, arguments );
			  }
		 }

		 [Obsolete]
		 public class Option<T>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly T ValueConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string MetadataConflict;

			  internal Option( T value, string metadata )
			  {
					this.ValueConflict = value;
					this.MetadataConflict = metadata;
			  }

			  public virtual T Value()
			  {
					return ValueConflict;
			  }

			  public virtual string Metadata()
			  {
					return MetadataConflict;
			  }

			  public override string ToString()
			  {
					return "Option[" + ValueConflict + ( !string.ReferenceEquals( MetadataConflict, null ) ? ", " + MetadataConflict : "" ) + "]";
			  }
		 }

		 private static readonly System.Func<string, Option<string>> _defaultOptionParser = from =>
		 {
		  int metadataStartIndex = from.IndexOf( OPTION_METADATA_DELIMITER );
		  return metadataStartIndex == -1 ? new Option<>( from, null ) : new Option<>( from.substring( 0, metadataStartIndex ), from.substring( metadataStartIndex + 1 ) );
		 };

		 private readonly string[] _args;
		 private readonly string[] _flags;
		 private readonly IDictionary<string, IList<Option<string>>> _map = new Dictionary<string, IList<Option<string>>>();
		 private readonly IList<string> _orphans = new List<string>();

		 [Obsolete]
		 public static ArgsParser WithFlags( params string[] flags )
		 {
			  return new ArgsParser( flags );
		 }

		 [Obsolete]
		 public static Args Parse( params string[] args )
		 {
			  return WithFlags().parse(args);
		 }

		 /// <summary>
		 /// Suitable for main( String[] args ) </summary>
		 /// <param name="args"> the arguments to parse. </param>
		 private Args( string[] flags, string[] args ) : this( _defaultOptionParser, flags, args )
		 {
		 }

		 /// <summary>
		 /// Suitable for main( String[] args ) </summary>
		 /// <param name="flags"> list of possible flags (e.g -v or -skip-bad). A flag differs from an option in that it
		 /// has no value after it. This list of flags is used for distinguishing between the two. </param>
		 /// <param name="args"> the arguments to parse. </param>
		 private Args( System.Func<string, Option<string>> optionParser, string[] flags, string[] args )
		 {
			  this._flags = flags;
			  this._args = args;
			  ParseArgs( optionParser, args );
		 }

		 [Obsolete]
		 public Args( IDictionary<string, string> source ) : this( _defaultOptionParser, source )
		 {
		 }

		 [Obsolete]
		 public Args( System.Func<string, Option<string>> optionParser, IDictionary<string, string> source )
		 {
			  this._flags = new string[] {};
			  this._args = null;
			  foreach ( KeyValuePair<string, string> entry in source.SetOfKeyValuePairs() )
			  {
					Put( optionParser, entry.Key, entry.Value );
			  }
		 }

		 [Obsolete]
		 public virtual string[] Source()
		 {
			  return this._args;
		 }

		 [Obsolete]
		 public virtual IDictionary<string, string> AsMap()
		 {
			  IDictionary<string, string> result = new Dictionary<string, string>();
			  foreach ( KeyValuePair<string, IList<Option<string>>> entry in _map.SetOfKeyValuePairs() )
			  {
					Option<string> value = Iterables.firstOrNull( entry.Value );
					result[entry.Key] = value != null ? value.Value() : null;
			  }
			  return result;
		 }

		 [Obsolete]
		 public virtual bool Has( string key )
		 {
			  return this._map.ContainsKey( key );
		 }

		 [Obsolete]
		 public virtual bool HasNonNull( string key )
		 {
			  IList<Option<string>> values = this._map[key];
			  return values != null && values.Count > 0;
		 }

		 [Obsolete]
		 private string GetSingleOptionOrNull( string key )
		 {
			  IList<Option<string>> values = this._map[key];
			  if ( values == null || values.Count == 0 )
			  {
					return null;
			  }
			  else if ( values.Count > 1 )
			  {
					throw new System.ArgumentException( "There are multiple values for '" + key + "'" );
			  }
			  return values[0].Value();
		 }

		 /// <summary>
		 /// Get a config option by name. </summary>
		 /// <param name="key"> name of the option, without any `-` or `--` prefix, eg. "o". </param>
		 /// <returns> the string value of the option, or null if the user has not specified it </returns>
		 [Obsolete]
		 public virtual string Get( string key )
		 {
			  return GetSingleOptionOrNull( key );
		 }

		 [Obsolete]
		 public virtual string Get( string key, string defaultValue )
		 {
			  string value = GetSingleOptionOrNull( key );
			  return !string.ReferenceEquals( value, null ) ? value : defaultValue;
		 }

		 [Obsolete]
		 public virtual string Get( string key, string defaultValueIfNotFound, string defaultValueIfNoValue )
		 {
			  string value = GetSingleOptionOrNull( key );
			  if ( !string.ReferenceEquals( value, null ) )
			  {
					return value;
			  }
			  return this._map.ContainsKey( key ) ? defaultValueIfNoValue : defaultValueIfNotFound;
		 }

		 [Obsolete]
		 public virtual Number GetNumber( string key, Number defaultValue )
		 {
			  string value = GetSingleOptionOrNull( key );
			  return !string.ReferenceEquals( value, null ) ? Convert.ToDouble( value ) : defaultValue;
		 }

		 [Obsolete]
		 public virtual long GetDuration( string key, long defaultValueInMillis )
		 {
			  string value = GetSingleOptionOrNull( key );
			  return !string.ReferenceEquals( value, null ) ? TimeUtil.ParseTimeMillis.apply( value ) : defaultValueInMillis;
		 }

		 /// <summary>
		 /// Like calling <seealso cref="getBoolean(string, Boolean)"/> with {@code false} for default value.
		 /// This is the most common case, i.e. returns {@code true} if boolean argument was specified as:
		 /// <ul>
		 /// <li>--myboolean</li>
		 /// <li>--myboolean=true</li>
		 /// </ul>
		 /// Otherwise {@code false.
		 /// } </summary>
		 /// <param name="key"> argument key. </param>
		 /// <returns> {@code true} if argument was specified w/o value, or w/ value {@code true}, otherwise {@code false}. </returns>
		 [Obsolete]
		 public virtual bool GetBoolean( string key )
		 {
			  return GetBoolean( key, false ).Value;
		 }

		 /// <summary>
		 /// Like calling <seealso cref="getBoolean(string, Boolean, Boolean)"/> with {@code true} for
		 /// {@code defaultValueIfNoValue}, i.e. specifying {@code --myarg} will interpret it as {@code true}.
		 /// </summary>
		 /// <param name="key"> argument key. </param>
		 /// <param name="defaultValueIfNotSpecified"> used if argument not specified. </param>
		 /// <returns> argument boolean value depending on what was specified, see above. </returns>
		 [Obsolete]
		 public virtual bool? GetBoolean( string key, bool? defaultValueIfNotSpecified )
		 {
			  return GetBoolean( key, defaultValueIfNotSpecified, true );
		 }

		 /// <summary>
		 /// Parses a {@code boolean} argument. There are a couple of cases:
		 /// <ul>
		 /// <li>The argument isn't specified. In this case the value of {@code defaultValueIfNotSpecified}
		 /// will be returned.</li>
		 /// <li>The argument is specified without value, for example <pre>--myboolean</pre>. In this case
		 /// the value of {@code defaultValueIfNotSpecified} will be returned.</li>
		 /// <li>The argument is specified with value, for example <pre>--myboolean=true</pre>.
		 /// In this case the actual value will be returned.</li>
		 /// </ul>
		 /// </summary>
		 /// <param name="key"> argument key. </param>
		 /// <param name="defaultValueIfNotSpecified"> used if argument not specified. </param>
		 /// <param name="defaultValueIfSpecifiedButNoValue"> used if argument specified w/o value. </param>
		 /// <returns> argument boolean value depending on what was specified, see above. </returns>
		 [Obsolete]
		 public virtual bool? GetBoolean( string key, bool? defaultValueIfNotSpecified, bool? defaultValueIfSpecifiedButNoValue )
		 {
			  string value = GetSingleOptionOrNull( key );
			  if ( !string.ReferenceEquals( value, null ) )
			  {
					return Convert.ToBoolean( value );
			  }
			  return this._map.ContainsKey( key ) ? defaultValueIfSpecifiedButNoValue : defaultValueIfNotSpecified;
		 }

		 [Obsolete]
		 public virtual T GetEnum<T>( Type enumClass, string key, T defaultValue ) where T : Enum<T>
		 {
				 enumClass = typeof( T );
			  string raw = GetSingleOptionOrNull( key );
			  if ( string.ReferenceEquals( raw, null ) )
			  {
					return defaultValue;
			  }

			  foreach ( T candidate in enumClass.EnumConstants )
			  {
					if ( candidate.name().Equals(raw) )
					{
						 return candidate;
					}
			  }
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  throw new System.ArgumentException( "No enum instance '" + raw + "' in " + enumClass.FullName );
		 }

		 /// <summary>
		 /// Orphans are arguments specified without options flags, eg:
		 /// 
		 /// <pre>myprogram -o blah orphan1 orphan2</pre>
		 /// 
		 /// Would yield a list here of {@code "orphan1"} and {@code "orphan2"}.
		 /// </summary>
		 /// <returns> list of orphan arguments </returns>
		 [Obsolete]
		 public virtual IList<string> Orphans()
		 {
			  return new List<string>( this._orphans );
		 }

		 /// <seealso cref= #orphans() </seealso>
		 /// <returns> list of orphan arguments. </returns>
		 [Obsolete]
		 public virtual string[] OrphansAsArray()
		 {
			  return _orphans.ToArray();
		 }

		 [Obsolete]
		 public virtual string[] AsArgs()
		 {
			  IList<string> list = new List<string>( _orphans.Count );
			  foreach ( string orphan in _orphans )
			  {
					string quote = orphan.Contains( " " ) ? " " : "";
					list.Add( quote + orphan + quote );
			  }
			  foreach ( KeyValuePair<string, IList<Option<string>>> entry in _map.SetOfKeyValuePairs() )
			  {
					foreach ( Option<string> option in entry.Value )
					{
						 string key = !string.ReferenceEquals( option.MetadataConflict, null ) ? entry.Key + OPTION_METADATA_DELIMITER + option.MetadataConflict() : entry.Key;
						 string value = option.Value();
						 string quote = key.Contains( " " ) || ( !string.ReferenceEquals( value, null ) && value.Contains( " " ) ) ? " " : "";
						 list.Add( quote + ( key.Length > 1 ? "--" : "-" ) + key + ( !string.ReferenceEquals( value, null ) ? "=" + value + quote : "" ) );
					}
			  }
			  return list.ToArray();
		 }

		 public override string ToString()
		 {
			  StringBuilder builder = new StringBuilder();
			  foreach ( string arg in AsArgs() )
			  {
					builder.Append( builder.Length > 0 ? " " : "" ).Append( arg );
			  }
			  return builder.ToString();
		 }

		 private static bool IsOption( string arg )
		 {
			  return arg.StartsWith( "-", StringComparison.Ordinal ) && arg.Length > 1;
		 }

		 private bool IsFlag( string arg )
		 {
			  return ArrayUtil.Contains( _flags, arg );
		 }

		 private static bool IsBoolean( string value )
		 {
			  return "true".Equals( value, StringComparison.OrdinalIgnoreCase ) || "false".Equals( value, StringComparison.OrdinalIgnoreCase );
		 }

		 private static string StripOption( string arg )
		 {
			  while ( arg.Length > 0 && arg[0] == '-' )
			  {
					arg = arg.Substring( 1 );
			  }
			  return arg;
		 }

		 private void ParseArgs( System.Func<string, Option<string>> optionParser, string[] args )
		 {
			  for ( int i = 0; i < args.Length; i++ )
			  {
					string arg = args[i];
					if ( IsOption( arg ) )
					{
						 arg = StripOption( arg );
						 int equalIndex = arg.IndexOf( '=' );
						 if ( equalIndex != -1 )
						 {
							  string key = arg.Substring( 0, equalIndex );
							  string value = arg.Substring( equalIndex + 1 );
							  if ( value.Length > 0 )
							  {
									Put( optionParser, key, value );
							  }
						 }
						 else if ( IsFlag( arg ) )
						 {
							  int nextIndex = i + 1;
							  string value = nextIndex < args.Length ? args[nextIndex] : null;
							  if ( IsBoolean( value ) )
							  {
									i = nextIndex;
									Put( optionParser, arg, Convert.ToBoolean( value ).ToString() );
							  }
							  else
							  {
									Put( optionParser, arg, null );
							  }
						 }
						 else
						 {
							  int nextIndex = i + 1;
							  string value = nextIndex < args.Length ? args[nextIndex] : null;
							  value = ( string.ReferenceEquals( value, null ) || IsOption( value ) ) ? null : value;
							  if ( !string.ReferenceEquals( value, null ) )
							  {
									i = nextIndex;
							  }
							  Put( optionParser, arg, value );
						 }
					}
					else
					{
						 _orphans.Add( arg );
					}
			  }
		 }

		 private void Put( System.Func<string, Option<string>> optionParser, string key, string value )
		 {
			  Option<string> option = optionParser( key );
			  IList<Option<string>> values = _map.computeIfAbsent( option.Value(), k => new List<Option<string>>() );
			  values.Add( new Option<>( value, option.Metadata() ) );
		 }

		 [Obsolete]
		 public static string JarUsage( Type main, params string[] @params )
		 {
			  StringBuilder usage = new StringBuilder( "USAGE: java [-cp ...] " );
			  try
			  {
					string jar = main.ProtectionDomain.CodeSource.Location.Path;
					usage.Append( "-jar " ).Append( jar );
			  }
			  catch ( Exception )
			  {
			  }
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
			  usage.Append( ' ' ).Append( main.FullName );
			  foreach ( string param in @params )
			  {
					usage.Append( ' ' ).Append( param );
			  }
			  return usage.ToString();
		 }

		 /// <summary>
		 /// Useful for printing usage where the description itself shouldn't have knowledge about the width
		 /// of the window where it's printed. Existing new-line characters in the text are honored.
		 /// </summary>
		 /// <param name="description"> text to split, if needed. </param>
		 /// <param name="maxLength"> line length to split at, actually closest previous white space. </param>
		 /// <returns> description split into multiple lines if need be. </returns>
		 [Obsolete]
		 public static string[] SplitLongLine( string description, int maxLength )
		 {
			  IList<string> lines = new List<string>();
			  while ( description.Length > 0 )
			  {
					string line = description.Substring( 0, Math.Min( maxLength, description.Length ) );
					int position = line.IndexOf( "\n", StringComparison.Ordinal );
					if ( position > -1 )
					{
						 line = description.Substring( 0, position );
						 lines.Add( line );
						 description = description.Substring( position );
						 if ( description.Length > 0 )
						 {
							  description = description.Substring( 1 );
						 }
					}
					else
					{
						 position = description.Length > maxLength ? FindSpaceBefore( description, maxLength ) : description.Length;
						 line = description.Substring( 0, position );
						 lines.Add( line );
						 description = description.Substring( position );
					}
			  }
			  return lines.ToArray();
		 }

		 private static int FindSpaceBefore( string description, int position )
		 {
			  while ( !char.IsWhiteSpace( description[position] ) )
			  {
					position--;
			  }
			  return position + 1;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @SafeVarargs public final <T> T interpretOption(String key, System.Func<String,T> defaultValue, System.Func<String,T> converter, org.neo4j.kernel.impl.util.Validator<T>... validators)
		 [Obsolete]
		 public T InterpretOption<T>( string key, System.Func<string, T> defaultValue, System.Func<string, T> converter, params Validator<T>[] validators )
		 {
			  T value;
			  if ( !Has( key ) )
			  {
					value = defaultValue( key );
			  }
			  else
			  {
					string stringValue = Get( key );
					value = converter( stringValue );
			  }
			  return Validated( value, validators );
		 }

		 /// <summary>
		 /// An option can be specified multiple times; this method will allow interpreting all values for
		 /// the given key, returning a <seealso cref="System.Collections.ICollection"/>. This is the only means of extracting multiple values
		 /// for any given option. All other methods revolve around zero or one value for an option.
		 /// </summary>
		 /// <param name="key"> Key of the option </param>
		 /// <param name="defaultValue"> Default value value of the option </param>
		 /// <param name="converter"> Converter to use </param>
		 /// <param name="validators"> Validators to use </param>
		 /// @param <T> The type of the option values </param>
		 /// <returns> The option values </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @SafeVarargs public final <T> java.util.Collection<T> interpretOptions(String key, System.Func<String,T> defaultValue, System.Func<String,T> converter, org.neo4j.kernel.impl.util.Validator<T>... validators)
		 [Obsolete]
		 public ICollection<T> InterpretOptions<T>( string key, System.Func<string, T> defaultValue, System.Func<string, T> converter, params Validator<T>[] validators )
		 {
			  ICollection<Option<T>> options = InterpretOptionsWithMetadata( key, defaultValue, converter, validators );
			  ICollection<T> values = new List<T>( options.Count );
			  foreach ( Option<T> option in options )
			  {
					values.Add( option.Value() );
			  }
			  return values;
		 }

		 /// <summary>
		 /// An option can be specified multiple times; this method will allow interpreting all values for
		 /// the given key, returning a <seealso cref="System.Collections.ICollection"/>. This is the only means of extracting multiple values
		 /// for any given option. All other methods revolve around zero or one value for an option.
		 /// This is also the only means of extracting metadata about a options. Metadata can be supplied as part
		 /// of the option key, like --my-option:Metadata "my value".
		 /// </summary>
		 /// <param name="key"> Key of the option </param>
		 /// <param name="defaultValue"> Default value value of the option </param>
		 /// <param name="converter"> Converter to use </param>
		 /// <param name="validators"> Validators to use </param>
		 /// @param <T> The type of the option values </param>
		 /// <returns> The option values </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @SafeVarargs public final <T> java.util.Collection<Option<T>> interpretOptionsWithMetadata(String key, System.Func<String,T> defaultValue, System.Func<String,T> converter, org.neo4j.kernel.impl.util.Validator<T>... validators)
		 [Obsolete]
		 public ICollection<Option<T>> InterpretOptionsWithMetadata<T>( string key, System.Func<string, T> defaultValue, System.Func<string, T> converter, params Validator<T>[] validators )
		 {
			  ICollection<Option<T>> values = new List<Option<T>>();
			  if ( !HasNonNull( key ) )
			  {
					T defaultItem = defaultValue( key );
					if ( defaultItem != null )
					{
						 values.Add( new Option<>( Validated( defaultItem, validators ), null ) );
					}
			  }
			  else
			  {
					foreach ( Option<string> option in _map[key] )
					{
						 string stringValue = option.Value();
						 values.Add( new Option<>( Validated( converter( stringValue ), validators ), option.Metadata() ) );
					}
			  }
			  return values;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @SafeVarargs public final <T> T interpretOrphan(int index, System.Func<String,T> defaultValue, System.Func<String,T> converter, org.neo4j.kernel.impl.util.Validator<T>... validators)
		 [Obsolete]
		 public T InterpretOrphan<T>( int index, System.Func<string, T> defaultValue, System.Func<string, T> converter, params Validator<T>[] validators )
		 {
			  Debug.Assert( index >= 0 );

			  T value;
			  if ( index >= _orphans.Count )
			  {
					value = defaultValue( "argument at index " + index );
			  }
			  else
			  {
					string stringValue = _orphans[index];
					value = converter( stringValue );
			  }
			  return Validated( value, validators );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private final <T> T validated(T value, org.neo4j.kernel.impl.util.Validator<T>... validators)
		 private T Validated<T>( T value, params Validator<T>[] validators )
		 {
			  if ( value != null )
			  {
					foreach ( Validator<T> validator in validators )
					{
						 validator.Validate( value );
					}
			  }
			  return value;
		 }
	}

}