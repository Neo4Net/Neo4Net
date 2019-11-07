using System;
using System.Collections.Generic;

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
namespace Neo4Net.@unsafe.Impl.Batchimport.input.csv
{

	using Neo4Net.Collections;
	using CharReadable = Neo4Net.Csv.Reader.CharReadable;
	using CharSeeker = Neo4Net.Csv.Reader.CharSeeker;
	using Neo4Net.Csv.Reader;
	using Extractors = Neo4Net.Csv.Reader.Extractors;
	using Mark = Neo4Net.Csv.Reader.Mark;
	using Neo4Net.Functions;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using Neo4Net.Collections.Helpers;
	using Entry = Neo4Net.@unsafe.Impl.Batchimport.input.csv.Header.Entry;
	using CSVHeaderInformation = Neo4Net.Values.Storable.CSVHeaderInformation;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using Neo4Net.Values.Storable;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.csv.reader.Readables.individualFiles;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.csv.reader.Readables.iterator;

	/// <summary>
	/// Provides common implementations of factories required by f.ex <seealso cref="CsvInput"/>.
	/// </summary>
	public class DataFactories
	{
		 private DataFactories()
		 {
		 }

		 /// <summary>
		 /// Creates a <seealso cref="DataFactory"/> where data exists in multiple files. If the first line of the first file is a header,
		 /// <seealso cref="defaultFormatNodeFileHeader()"/> can be used to extract that.
		 /// </summary>
		 /// <param name="decorator"> Decorator for this data. </param>
		 /// <param name="charset"> <seealso cref="Charset"/> to read data in. </param>
		 /// <param name="files"> the files making up the data.
		 /// </param>
		 /// <returns> <seealso cref="DataFactory"/> that returns a <seealso cref="CharSeeker"/> over all the supplied {@code files}. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static DataFactory data(final Decorator decorator, final java.nio.charset.Charset charset, final java.io.File... files)
		 public static DataFactory Data( Decorator decorator, Charset charset, params File[] files )
		 {
			  if ( Files.Length == 0 )
			  {
					throw new System.ArgumentException( "No files specified" );
			  }

			  return config => new DataAnonymousInnerClass( decorator, charset, files );
		 }

		 private class DataAnonymousInnerClass : Data
		 {
			 private Neo4Net.@unsafe.Impl.Batchimport.input.csv.Decorator _decorator;
			 private Charset _charset;
			 private File[] _files;

			 public DataAnonymousInnerClass( Neo4Net.@unsafe.Impl.Batchimport.input.csv.Decorator decorator, Charset charset, File[] files )
			 {
				 this._decorator = decorator;
				 this._charset = charset;
				 this._files = files;
			 }

			 public RawIterator<CharReadable, IOException> stream()
			 {
				  return individualFiles( _charset, _files );
			 }

			 public Decorator decorator()
			 {
				  return _decorator;
			 }
		 }

		 /// <param name="decorator"> Decorator for this data. </param>
		 /// <param name="readable"> we need to have this as a <seealso cref="Factory"/> since one data file may be opened and scanned
		 /// multiple times. </param>
		 /// <returns> <seealso cref="DataFactory"/> that returns a <seealso cref="CharSeeker"/> over the supplied {@code readable} </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static DataFactory data(final Decorator decorator, final System.Func<Neo4Net.csv.reader.CharReadable> readable)
		 public static DataFactory Data( Decorator decorator, System.Func<CharReadable> readable )
		 {
			  return config => new DataAnonymousInnerClass2( decorator, readable );
		 }

		 private class DataAnonymousInnerClass2 : Data
		 {
			 private Neo4Net.@unsafe.Impl.Batchimport.input.csv.Decorator _decorator;
			 private System.Func<CharReadable> _readable;

			 public DataAnonymousInnerClass2( Neo4Net.@unsafe.Impl.Batchimport.input.csv.Decorator decorator, System.Func<CharReadable> readable )
			 {
				 this._decorator = decorator;
				 this._readable = readable;
			 }

			 public RawIterator<CharReadable, IOException> stream()
			 {
				  return iterator( reader => reader, _readable() );
			 }

			 public Decorator decorator()
			 {
				  return _decorator;
			 }
		 }

		 /// <summary>
		 /// Header parser that will read header information, using the default node header format,
		 /// from the top of the data file.
		 /// 
		 /// This header factory can be used even when the header exists in a separate file, if that file
		 /// is the first in the list of files supplied to <seealso cref="data"/>.
		 /// </summary>
		 /// <param name="defaultTimeZone"> A supplier of the time zone to be used for temporal values when not specified explicitly </param>
		 public static Header.Factory DefaultFormatNodeFileHeader( System.Func<ZoneId> defaultTimeZone )
		 {
			  return new DefaultNodeFileHeaderParser( defaultTimeZone );
		 }

		 /// <summary>
		 /// Like <seealso cref="defaultFormatNodeFileHeader(Supplier<ZoneId>)"/> with UTC as the default time zone.
		 /// </summary>
		 public static Header.Factory DefaultFormatNodeFileHeader()
		 {
			  return DefaultFormatNodeFileHeader( _defaultTimeZone );
		 }

		 /// <summary>
		 /// Header parser that will read header information, using the default relationship header format,
		 /// from the top of the data file.
		 /// 
		 /// This header factory can be used even when the header exists in a separate file, if that file
		 /// is the first in the list of files supplied to <seealso cref="data"/>.
		 /// </summary>
		 /// <param name="defaultTimeZone"> A supplier of the time zone to be used for temporal values when not specified explicitly </param>
		 public static Header.Factory DefaultFormatRelationshipFileHeader( System.Func<ZoneId> defaultTimeZone )
		 {
			  return new DefaultRelationshipFileHeaderParser( defaultTimeZone );
		 }

		 /// <summary>
		 /// Like <seealso cref="defaultFormatRelationshipFileHeader(Supplier<ZoneId>)"/> with UTC as the default time zone.
		 /// </summary>
		 public static Header.Factory DefaultFormatRelationshipFileHeader()
		 {
			  return DefaultFormatRelationshipFileHeader( _defaultTimeZone );
		 }

		 private static System.Func<ZoneId> _defaultTimeZone = () => UTC;

		 private abstract class AbstractDefaultFileHeaderParser : Header.Factory
		 {
			  internal readonly bool CreateGroups;
			  internal readonly Type[] MandatoryTypes;
			  internal readonly System.Func<ZoneId> DefaultTimeZone;

			  protected internal AbstractDefaultFileHeaderParser( System.Func<ZoneId> defaultTimeZone, bool createGroups, params Type[] mandatoryTypes )
			  {
					this.DefaultTimeZone = defaultTimeZone;
					this.CreateGroups = createGroups;
					this.MandatoryTypes = mandatoryTypes;
			  }

			  public override Header Create( CharSeeker dataSeeker, Configuration config, IdType idType, Groups groups )
			  {
					try
					{
						 Mark mark = new Mark();
						 Extractors extractors = new Extractors( config.arrayDelimiter(), config.emptyQuotedStringsAsNull(), config.trimStrings(), DefaultTimeZone );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.csv.reader.Extractor<?> idExtractor = idType.extractor(extractors);
						 Extractor<object> idExtractor = idType.extractor( extractors );
						 int delimiter = config.delimiter();
						 IList<Header.Entry> columns = new List<Header.Entry>();
						 for ( int i = 0; !mark.EndOfLine && dataSeeker.Seek( mark, delimiter ); i++ )
						 {
							  string entryString = dataSeeker.TryExtract( mark, extractors.String() ) ? extractors.String().value() : null;
							  HeaderEntrySpec spec = new HeaderEntrySpec( entryString );

							  if ( ( string.ReferenceEquals( spec.Name, null ) && string.ReferenceEquals( spec.Type, null ) ) || ( !string.ReferenceEquals( spec.Type, null ) && spec.Type.Equals( Type.Ignore.name() ) ) )
							  {
									columns.Add( new Header.Entry( null, Type.Ignore, Neo4Net.@unsafe.Impl.Batchimport.input.Group_Fields.Global, null, null ) );
							  }
							  else
							  {
									Group group = CreateGroups ? groups.GetOrCreate( spec.GroupName ) : groups.Get( spec.GroupName );
									columns.Add( Entry( i, spec.Name, spec.Type, group, extractors, idExtractor ) );
							  }
						 }
						 Entry[] entries = columns.ToArray();
						 ValidateHeader( entries );
						 return new Header( entries );
					}
					catch ( IOException e )
					{
						 throw new Exception( e );
					}
			  }

			  internal virtual void ValidateHeader( Entry[] entries )
			  {
					IDictionary<string, Entry> properties = new Dictionary<string, Entry>();
					Dictionary<Type, Entry> singletonEntries = new Dictionary<Type, Entry>( typeof( Type ) );
					foreach ( Entry entry in entries )
					{
						 switch ( entry.Type() )
						 {
						 case PROPERTY:
							  Entry existingPropertyEntry = properties[entry.Name()];
							  if ( existingPropertyEntry != null )
							  {
									throw new DuplicateHeaderException( existingPropertyEntry, entry );
							  }
							  properties[entry.Name()] = entry;
							  break;

						 case ID:
					 case START_ID:
				 case END_ID:
			 case TYPE:
							  Entry existingSingletonEntry = singletonEntries[entry.Type()];
							  if ( existingSingletonEntry != null )
							  {
									throw new DuplicateHeaderException( existingSingletonEntry, entry );
							  }
							  singletonEntries[entry.Type()] = entry;
							  break;
						 default:
							  // No need to validate other headers
							  break;
						 }
					}

					foreach ( Type type in MandatoryTypes )
					{
						 if ( !singletonEntries.ContainsKey( type ) )
						 {
							  throw new HeaderException( format( "Missing header of type %s, among entries %s", type, Arrays.ToString( entries ) ) );
						 }
					}
			  }

			  protected internal virtual bool IsRecognizedType( string typeSpec )
			  {
					foreach ( Type type in Enum.GetValues( typeof( Type ) ) )
					{
						 if ( type.name().equalsIgnoreCase(typeSpec) )
						 {
							  return true;
						 }
					}
					return false;
			  }

			  public virtual bool Defined
			  {
				  get
				  {
						return false;
				  }
			  }

			  /// <param name="idExtractor"> we supply the id extractor explicitly because it's a configuration,
			  /// or at least input-global concern and not a concern of this particular header. </param>
			  protected internal abstract Header.Entry entry<T1>( int index, string name, string typeSpec, Group group, Extractors extractors, Extractor<T1> idExtractor );
		 }

		 private class HeaderEntrySpec
		 {
			  internal readonly string Name;
			  internal readonly string Type;
			  internal readonly string GroupName;

			  internal HeaderEntrySpec( string rawHeaderField )
			  {
					string name = rawHeaderField;
					string type = null;
					string groupName = null;

					int typeIndex;

					if ( !string.ReferenceEquals( rawHeaderField, null ) )
					{
						 string rawHeaderUntilOptions = rawHeaderField.Split( "\\{", true )[0];
						 if ( ( typeIndex = rawHeaderUntilOptions.LastIndexOf( ':' ) ) != -1 )
						 { // Specific type given
							  name = typeIndex > 0 ? rawHeaderField.Substring( 0, typeIndex ) : null;
							  type = rawHeaderField.Substring( typeIndex + 1 );
							  int groupNameStartIndex = type.IndexOf( '(' );
							  if ( groupNameStartIndex != -1 )
							  { // Specific group given also
									if ( !type.EndsWith( ")", StringComparison.Ordinal ) )
									{
										 throw new System.ArgumentException( "Group specification in '" + rawHeaderField + "' is invalid, format expected to be 'name:TYPE(group)' " + "where TYPE and (group) are optional" );
									}
									groupName = type.Substring( groupNameStartIndex + 1, ( type.Length - 1 ) - ( groupNameStartIndex + 1 ) );
									type = type.Substring( 0, groupNameStartIndex );
							  }
						 }
					}

					this.Name = name;
					this.Type = type;
					this.GroupName = groupName;
			  }
		 }

		 private class DefaultNodeFileHeaderParser : AbstractDefaultFileHeaderParser
		 {
			  protected internal DefaultNodeFileHeaderParser( System.Func<ZoneId> defaultTimeZone ) : base( defaultTimeZone, true )
			  {
			  }

			  protected internal override Header.Entry Entry<T1>( int index, string name, string typeSpec, Group group, Extractors extractors, Extractor<T1> idExtractor )
			  {
					// For nodes it's simply ID,LABEL,PROPERTY. typeSpec can be either ID,LABEL or a type of property,
					// like 'int' or 'string_array' or similar, or empty for 'string' property.
					Type type = null;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.csv.reader.Extractor<?> extractor = null;
					Extractor<object> extractor = null;
					CSVHeaderInformation optionalParameter = null;
					if ( string.ReferenceEquals( typeSpec, null ) )
					{
						 type = Type.Property;
						 extractor = extractors.String();
					}
					else
					{
						 Pair<string, string> split = SplitTypeSpecAndOptionalParameter( typeSpec );
						 typeSpec = split.First();
						 string optionalParameterString = split.Other();
						 if ( !string.ReferenceEquals( optionalParameterString, null ) )
						 {
							  if ( Extractors.PointExtractor.NAME.Equals( typeSpec ) )
							  {
									optionalParameter = PointValue.parseHeaderInformation( optionalParameterString );
							  }
							  else if ( Extractors.TimeExtractor.NAME.Equals( typeSpec ) || Extractors.DateTimeExtractor.NAME.Equals( typeSpec ) )
							  {
									optionalParameter = TemporalValue.parseHeaderInformation( optionalParameterString );
							  }
						 }
						 if ( typeSpec.Equals( Type.Id.name(), StringComparison.OrdinalIgnoreCase ) )
						 {
							  type = Type.Id;
							  extractor = idExtractor;
						 }
						 else if ( typeSpec.Equals( Type.Label.name(), StringComparison.OrdinalIgnoreCase ) )
						 {
							  type = Type.Label;
							  extractor = extractors.StringArray();
						 }
						 else if ( IsRecognizedType( typeSpec ) )
						 {
							  throw new HeaderException( "Unexpected node header type '" + typeSpec + "'" );
						 }
						 else
						 {
							  type = Type.Property;
							  extractor = ParsePropertyType( typeSpec, extractors );
						 }
					}
					return new Header.Entry( name, type, group, extractor, optionalParameter );
			  }
		 }

		 private class DefaultRelationshipFileHeaderParser : AbstractDefaultFileHeaderParser
		 {
			  protected internal DefaultRelationshipFileHeaderParser( System.Func<ZoneId> defaultTimeZone ) : base( defaultTimeZone, false, Type.StartId, Type.EndId )
			  {
					// Don't have TYPE as mandatory since a decorator could provide that
			  }

			  protected internal override Header.Entry Entry<T1>( int index, string name, string typeSpec, Group group, Extractors extractors, Extractor<T1> idExtractor )
			  {
					Type type = null;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.csv.reader.Extractor<?> extractor = null;
					Extractor<object> extractor = null;
					CSVHeaderInformation optionalParameter = null;
					if ( string.ReferenceEquals( typeSpec, null ) )
					{ // Property
						 type = Type.Property;
						 extractor = extractors.String();
					}
					else
					{
						 Pair<string, string> split = SplitTypeSpecAndOptionalParameter( typeSpec );
						 typeSpec = split.First();
						 string optionalParameterString = split.Other();
						 if ( !string.ReferenceEquals( optionalParameterString, null ) )
						 {
							  if ( Extractors.PointExtractor.NAME.Equals( typeSpec ) )
							  {
									optionalParameter = PointValue.parseHeaderInformation( optionalParameterString );
							  }
							  else if ( Extractors.TimeExtractor.NAME.Equals( typeSpec ) || Extractors.DateTimeExtractor.NAME.Equals( typeSpec ) )
							  {
									optionalParameter = TemporalValue.parseHeaderInformation( optionalParameterString );
							  }
						 }

						 if ( typeSpec.Equals( Type.StartId.name(), StringComparison.OrdinalIgnoreCase ) )
						 {
							  type = Type.StartId;
							  extractor = idExtractor;
						 }
						 else if ( typeSpec.Equals( Type.EndId.name(), StringComparison.OrdinalIgnoreCase ) )
						 {
							  type = Type.EndId;
							  extractor = idExtractor;
						 }
						 else if ( typeSpec.Equals( Type.Type.name(), StringComparison.OrdinalIgnoreCase ) )
						 {
							  type = Type.Type;
							  extractor = extractors.String();
						 }
						 else if ( IsRecognizedType( typeSpec ) )
						 {
							  throw new HeaderException( "Unexpected relationship header type '" + typeSpec + "'" );
						 }
						 else
						 {
							  type = Type.Property;
							  extractor = ParsePropertyType( typeSpec, extractors );
						 }
					}
					return new Header.Entry( name, type, group, extractor, optionalParameter );
			  }

		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private static Neo4Net.csv.reader.Extractor<?> parsePropertyType(String typeSpec, Neo4Net.csv.reader.Extractors extractors)
		 private static Extractor<object> ParsePropertyType( string typeSpec, Extractors extractors )
		 {
			  try
			  {
					return extractors.ValueOf( typeSpec );
			  }
			  catch ( System.ArgumentException e )
			  {
					throw new HeaderException( "Unable to parse header", e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public static Iterable<DataFactory> datas(DataFactory... factories)
		 public static IEnumerable<DataFactory> Datas( params DataFactory[] factories )
		 {
			  return Iterables.iterable( factories );
		 }

		 private static Pattern _typeSpecAndOptionalParameter = Pattern.compile( "(?<newTypeSpec>.+?)(?<optionalParameter>\\{.*\\})?$" );

		 public static Pair<string, string> SplitTypeSpecAndOptionalParameter( string typeSpec )
		 {
			  string optionalParameter = null;
			  string newTypeSpec = typeSpec;

			  Matcher matcher = _typeSpecAndOptionalParameter.matcher( typeSpec );

			  if ( matcher.find() )
			  {
					try
					{
						 newTypeSpec = matcher.group( "newTypeSpec" );
						 optionalParameter = matcher.group( "optionalParameter" );
					}
					catch ( System.ArgumentException e )
					{
						 string errorMessage = format( "Failed to parse header: '%s'", typeSpec );
						 throw new System.ArgumentException( errorMessage, e );
					}
			  }
			  return Pair.of( newTypeSpec, optionalParameter );
		 }
	}

}