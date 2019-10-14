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
namespace Neo4Net.Server.rest.repr.formats
{

	using Service = Neo4Net.Helpers.Service;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.domain.JsonHelper.assertSupportedPropertyValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.domain.JsonHelper.readJson;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(RepresentationFormat.class) public class CompactJsonFormat extends org.neo4j.server.rest.repr.RepresentationFormat
	public class CompactJsonFormat : RepresentationFormat
	{
		 public static readonly MediaType MediaType = new MediaType( MediaType.APPLICATION_JSON_TYPE.Type, MediaType.APPLICATION_JSON_TYPE.Subtype, MapUtil.stringMap( "compact", "true" ) );

		 public CompactJsonFormat() : base(MediaType)
		 {
		 }

		 private abstract class MappingTemplate
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           NODE(org.neo4j.server.rest.repr.Representation.NODE) { String render(java.util.Map<String, Object> serialized) { return org.neo4j.server.rest.domain.JsonHelper.createJsonFrom(org.neo4j.helpers.collection.MapUtil.map("self", serialized.get("self"), "data", serialized.get("data"))); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           RELATIONSHIP(org.neo4j.server.rest.repr.Representation.RELATIONSHIP) { String render(java.util.Map<String, Object> serialized) { return org.neo4j.server.rest.domain.JsonHelper.createJsonFrom(org.neo4j.helpers.collection.MapUtil.map("self", serialized.get("self"), "data", serialized.get("data"))); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           STRING(org.neo4j.server.rest.repr.Representation.STRING) { String render(java.util.Map<String, Object> serialized) { return org.neo4j.server.rest.domain.JsonHelper.createJsonFrom(serialized); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           EXCEPTION(org.neo4j.server.rest.repr.Representation.EXCEPTION) { String render(java.util.Map<String, Object> data) { return org.neo4j.server.rest.domain.JsonHelper.createJsonFrom(data); } };

			  private static readonly IList<MappingTemplate> valueList = new List<MappingTemplate>();

			  public enum InnerEnum
			  {
				  NODE,
				  RELATIONSHIP,
				  STRING,
				  EXCEPTION
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private MappingTemplate( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }
			  internal readonly string key;

			  internal MappingTemplate( string name, InnerEnum innerEnum, string key )
			  {
					this._key = key;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal static readonly IDictionary<string, MappingTemplate> TEMPLATES = new Dictionary<string, MappingTemplate>();
			  static MappingTemplate()
			  {
					foreach ( MappingTemplate template in values() )
					{
						 Templates.put( template._key, template );
					}

				  valueList.Add( NODE );
				  valueList.Add( RELATIONSHIP );
				  valueList.Add( STRING );
				  valueList.Add( EXCEPTION );
			  }

			  internal abstract string render( IDictionary<string, object> data );

			 public static IList<MappingTemplate> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static MappingTemplate valueOf( string name )
			 {
				 foreach ( MappingTemplate enumInstance in MappingTemplate.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private class CompactJsonWriter : MapWrappingWriter
		 {
			  internal readonly MappingTemplate Template;

			  internal CompactJsonWriter( MappingTemplate template ) : base( new Dictionary<string, object>(), true )
			  {
					this.Template = template;
			  }

			  protected internal override MappingWriter NewMapping( string type, string key )
			  {
					IDictionary<string, object> map = new Dictionary<string, object>();
					Data[key] = map;
					return new MapWrappingWriter( map, InteractiveConflict );
			  }

			  protected internal override void WriteValue( string type, string key, object value )
			  {
					Data[key] = value;
			  }

			  protected internal override ListWriter NewList( string type, string key )
			  {
					IList<object> list = new List<object>();
					Data[key] = list;
					return new ListWrappingWriter( list, InteractiveConflict );
			  }

			  internal virtual string Complete()
			  {
					return Template.render( this.Data );
			  }

		 }

		 protected internal override ListWriter SerializeList( string type )
		 {
			  return new ListWrappingWriter( new List<object>() );
		 }

		 protected internal override string Complete( ListWriter serializer )
		 {
			  return JsonHelper.createJsonFrom( ( ( ListWrappingWriter ) serializer ).Data );
		 }

		 protected internal override MappingWriter SerializeMapping( string type )
		 {
			  MappingTemplate template = MappingTemplate.TEMPLATES.get( type );
			  if ( template == null )
			  {
					throw new WebApplicationException( Response.status( Response.Status.NOT_ACCEPTABLE ).entity( "Cannot represent \"" + type + "\" as compactJson" ).build() );
			  }
			  return new CompactJsonWriter( template );
		 }

		 protected internal override string Complete( MappingWriter serializer )
		 {
			  return ( ( CompactJsonWriter ) serializer ).Complete();
		 }

		 protected internal override string SerializeValue( string type, object value )
		 {
			  return JsonHelper.createJsonFrom( value );
		 }

		 private bool Empty( string input )
		 {
			  return string.ReferenceEquals( input, null ) || "".Equals( input.Trim() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Map<String, Object> readMap(String input, String... requiredKeys) throws org.neo4j.server.rest.repr.BadInputException
		 public override IDictionary<string, object> ReadMap( string input, params string[] requiredKeys )
		 {
			  if ( Empty( input ) )
			  {
					return DefaultFormat.validateKeys( Collections.emptyMap(), requiredKeys );
			  }
			  try
			  {
					return DefaultFormat.validateKeys( JsonHelper.jsonToMap( StripByteOrderMark( input ) ), requiredKeys );
			  }
			  catch ( JsonParseException ex )
			  {
					throw new BadInputException( ex );
			  }
		 }

		 public override IList<object> ReadList( string input )
		 {
			  // TODO tobias: Implement readList() [Dec 10, 2010]
			  throw new System.NotSupportedException( "Not implemented: JsonInput.readList()" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object readValue(String input) throws org.neo4j.server.rest.repr.BadInputException
		 public override object ReadValue( string input )
		 {
			  if ( Empty( input ) )
			  {
					return Collections.emptyMap();
			  }
			  try
			  {
					return assertSupportedPropertyValue( readJson( StripByteOrderMark( input ) ) );
			  }
			  catch ( JsonParseException ex )
			  {
					throw new BadInputException( ex );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.net.URI readUri(String input) throws org.neo4j.server.rest.repr.BadInputException
		 public override URI ReadUri( string input )
		 {
			  try
			  {
					return new URI( ReadValue( input ).ToString() );
			  }
			  catch ( URISyntaxException e )
			  {
					throw new BadInputException( e );
			  }
		 }

		 private string StripByteOrderMark( string @string )
		 {
			  if ( !string.ReferenceEquals( @string, null ) && @string.Length > 0 && @string[0] == ( char )0xfeff )
			  {
					return @string.Substring( 1 );
			  }
			  return @string;
		 }
	}

}