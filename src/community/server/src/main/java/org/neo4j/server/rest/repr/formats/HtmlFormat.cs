using System.Collections.Generic;
using System.Text;

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

	using HtmlHelper = Neo4Net.Server.rest.domain.HtmlHelper;

	public class HtmlFormat : RepresentationFormat
	{
		 public HtmlFormat() : base(MediaType.TEXT_HTML_TYPE)
		 {
		 }

		 private abstract class MappingTemplate
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           NODE(org.neo4j.server.rest.repr.Representation.NODE) { String render(java.util.Map<String, Object> serialized) { String javascript = ""; StringBuilder builder = org.neo4j.server.rest.domain.HtmlHelper.start(org.neo4j.server.rest.domain.HtmlHelper.ObjectType.NODE, javascript); org.neo4j.server.rest.domain.HtmlHelper.append(builder, java.util.Collections.singletonMap("data", serialized.get("data")), org.neo4j.server.rest.domain.HtmlHelper.ObjectType.NODE); builder.append("<form action='javascript:neo4jHtmlBrowse.getRelationships();'>"); builder.append("<fieldset><legend>Get relationships</legend>\n"); builder.append("<label for='direction'>with direction</label>\n" + "<select id='direction'>"); builder.append("<option value='").append(serialized.get("all_typed_relationships")).append("'>all</option>"); builder.append("<option value='").append(serialized.get("incoming_typed_relationships")).append("'>in</option>"); builder.append("<option value='").append(serialized.get("outgoing_typed_relationships")).append("'>out</option>"); builder.append("</select>\n"); builder.append("<label for='types'>for type(s)</label><select id='types' multiple='multiple'>"); for(String relationshipType : (java.util.List<String>) serialized.get("relationship_types")) { builder.append("<option selected='selected' value='").append(relationshipType).append("'>"); builder.append(relationshipType).append("</option>"); } builder.append("</select>\n"); builder.append("<button>Get</button>\n"); builder.append("</fieldset></form>\n"); return org.neo4j.server.rest.domain.HtmlHelper.end(builder); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           RELATIONSHIP(org.neo4j.server.rest.repr.Representation.RELATIONSHIP) { String render(java.util.Map<String, Object> serialized) { java.util.Map<Object, Object> map = new java.util.LinkedHashMap<>(); transfer(serialized, map, "type", "data", "start", "end"); return org.neo4j.server.rest.domain.HtmlHelper.from(map, org.neo4j.server.rest.domain.HtmlHelper.ObjectType.RELATIONSHIP); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           NODE_INDEXES(org.neo4j.server.rest.repr.Representation.NODE_INDEXES) { String render(java.util.Map<String, Object> serialized) { return renderIndex(serialized); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           RELATIONSHIP_INDEXES(org.neo4j.server.rest.repr.Representation.RELATIONSHIP_INDEXES) { String render(java.util.Map<String, Object> serialized) { return renderIndex(serialized); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           GRAPHDB(org.neo4j.server.rest.repr.Representation.GRAPHDB) { String render(java.util.Map<String, Object> serialized) { java.util.Map<Object, Object> map = new java.util.HashMap<>(); transfer(serialized, map, "index", "node_index", "relationship_index"); return org.neo4j.server.rest.domain.HtmlHelper.from(map, org.neo4j.server.rest.domain.HtmlHelper.ObjectType.ROOT); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           EXCEPTION(org.neo4j.server.rest.repr.Representation.EXCEPTION) { String render(java.util.Map<String, Object> serialized) { StringBuilder entity = new StringBuilder("<html>"); entity.append("<head><title>Error</title></head><body>"); Object subjectOrNull = serialized.get("message"); if(subjectOrNull != null) { entity.append("<p><pre>").append(subjectOrNull).append("</pre></p>"); } entity.append("<p><pre>").append(serialized.get("exception")); java.util.List<Object> tb = (java.util.List<Object>) serialized.get("stackTrace"); if(tb != null) { for(Object el : tb) { entity.append("\n\tat " + el); } } entity.append("</pre></p>").append("</body></html>"); return entity.toString(); } };

			  private static readonly IList<MappingTemplate> valueList = new List<MappingTemplate>();

			  public enum InnerEnum
			  {
				  NODE,
				  RELATIONSHIP,
				  NODE_INDEXES,
				  RELATIONSHIP_INDEXES,
				  GRAPHDB,
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
				  valueList.Add( NODE_INDEXES );
				  valueList.Add( RELATIONSHIP_INDEXES );
				  valueList.Add( GRAPHDB );
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

		 private abstract class ListTemplate
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           NODES { String render(java.util.List<Object> data) { StringBuilder builder = org.neo4j.server.rest.domain.HtmlHelper.start("Index hits", null); if(data.isEmpty()) { org.neo4j.server.rest.domain.HtmlHelper.appendMessage(builder, "No index hits"); return org.neo4j.server.rest.domain.HtmlHelper.end(builder); } else { for(java.util.Map<JavaToDotNetGenericWildcard, JavaToDotNetGenericWildcard> serialized : (java.util.List<java.util.Map<JavaToDotNetGenericWildcard, JavaToDotNetGenericWildcard>>)(java.util.List<JavaToDotNetGenericWildcard>) data) { java.util.Map<Object, Object> map = new java.util.LinkedHashMap<>(); transfer(serialized, map, "self", "data"); org.neo4j.server.rest.domain.HtmlHelper.append(builder, map, org.neo4j.server.rest.domain.HtmlHelper.ObjectType.NODE); } return org.neo4j.server.rest.domain.HtmlHelper.end(builder); } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           RELATIONSHIPS { String render(java.util.List<Object> data) { if(data.isEmpty()) { StringBuilder builder = org.neo4j.server.rest.domain.HtmlHelper.start(org.neo4j.server.rest.domain.HtmlHelper.ObjectType.RELATIONSHIP, null); org.neo4j.server.rest.domain.HtmlHelper.appendMessage(builder, "No relationships found"); return org.neo4j.server.rest.domain.HtmlHelper.end(builder); } else { java.util.Collection<Object> list = new java.util.ArrayList<>(); for(java.util.Map<JavaToDotNetGenericWildcard, JavaToDotNetGenericWildcard> serialized : (java.util.List<java.util.Map<JavaToDotNetGenericWildcard, JavaToDotNetGenericWildcard>>)(java.util.List<JavaToDotNetGenericWildcard>) data) { java.util.Map<Object, Object> map = new java.util.LinkedHashMap<>(); transfer(serialized, map, "self", "type", "data", "start", "end"); list.add(map); } return org.neo4j.server.rest.domain.HtmlHelper.from(list, org.neo4j.server.rest.domain.HtmlHelper.ObjectType.RELATIONSHIP); } } };

			  private static readonly IList<ListTemplate> valueList = new List<ListTemplate>();

			  static ListTemplate()
			  {
				  valueList.Add( NODES );
				  valueList.Add( RELATIONSHIPS );
			  }

			  public enum InnerEnum
			  {
				  NODES,
				  RELATIONSHIPS
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private ListTemplate( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal abstract string render( IList<object> data );

			 public static IList<ListTemplate> values()
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

			 public static ListTemplate valueOf( string name )
			 {
				 foreach ( ListTemplate enumInstance in ListTemplate.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private static void Transfer<T1>( IDictionary<T1> from, IDictionary<object, object> to, params string[] keys )
		 {
			  foreach ( string key in keys )
			  {
					object value = from[key];
					if ( value != null )
					{
						 to[key] = value;
					}
			  }
		 }

		 private static string RenderIndex( IDictionary<string, object> serialized )
		 {
			  string javascript = "";
			  StringBuilder builder = HtmlHelper.start( HtmlHelper.ObjectType.INDEX_ROOT, javascript );
			  int counter = 0;
			  foreach ( string indexName in serialized.Keys )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<?, ?> indexMapObject = (java.util.Map<?, ?>) serialized.get(indexName);
					IDictionary<object, ?> indexMapObject = ( IDictionary<object, ?> ) serialized[indexName];
					builder.Append( "<ul>" );
					{
						 builder.Append( "<li>" );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<?, ?> indexMap = indexMapObject;
						 IDictionary<object, ?> indexMap = indexMapObject;
						 string keyId = "key_" + counter;
						 string valueId = "value_" + counter;
						 builder.Append( "<form action='javascript:neo4jHtmlBrowse.search(\"" ).Append( indexMap["template"] ).Append( "\",\"" ).Append( keyId ).Append( "\",\"" ).Append( valueId ).Append( "\");'><fieldset><legend> name: " ).Append( indexName ).Append( " (configuration: " ).Append( indexMap["type"] ).Append( ")</legend>\n" );
						 builder.Append( "<label for='" ).Append( keyId ).Append( "'>Key</label><input id='" ).Append( keyId ).Append( "'>\n" );
						 builder.Append( "<label for='" ).Append( valueId ).Append( "'>Value</label><input id='" ).Append( valueId ).Append( "'>\n" );
						 builder.Append( "<button>Search</button>\n" );
						 builder.Append( "</fieldset></form>\n" );
						 builder.Append( "</li>\n" );
						 counter++;
					}
					builder.Append( "</ul>" );
			  }
			  return HtmlHelper.end( builder );
		 }

		 private class HtmlMap : MapWrappingWriter
		 {
			  internal readonly MappingTemplate Template;

			  internal HtmlMap( MappingTemplate template ) : base( new Dictionary<string, object>(), true )
			  {
					this.Template = template;
			  }

			  internal virtual string Complete()
			  {
					return Template.render( this.Data );
			  }
		 }

		 private class HtmlList : ListWrappingWriter
		 {
			  internal readonly ListTemplate Template;

			  internal HtmlList( ListTemplate template ) : base( new List<object>(), true )
			  {
					this.Template = template;
			  }

			  internal virtual string Complete()
			  {
					return Template.render( this.Data );
			  }
		 }

		 protected internal override string Complete( ListWriter serializer )
		 {
			  return ( ( HtmlList ) serializer ).Complete();
		 }

		 protected internal override string Complete( MappingWriter serializer )
		 {
			  return ( ( HtmlMap ) serializer ).Complete();
		 }

		 protected internal override ListWriter SerializeList( string type )
		 {
			  if ( Representation.NODE_LIST.Equals( type ) )
			  {
					return new HtmlList( ListTemplate.Nodes );
			  }
			  else if ( Representation.RELATIONSHIP_LIST.Equals( type ) )
			  {
					return new HtmlList( ListTemplate.Relationships );
			  }
			  else
			  {
					throw new WebApplicationException( Response.status( Response.Status.NOT_ACCEPTABLE ).entity( "Cannot represent \"" + type + "\" as html" ).build() );
			  }
		 }

		 protected internal override MappingWriter SerializeMapping( string type )
		 {
			  MappingTemplate template = MappingTemplate.TEMPLATES.get( type );
			  if ( template == null )
			  {
					throw new WebApplicationException( Response.status( Response.Status.NOT_ACCEPTABLE ).entity( "Cannot represent \"" + type + "\" as html" ).build() );
			  }
			  return new HtmlMap( template );
		 }

		 protected internal override string SerializeValue( string type, object value )
		 {
			  throw new WebApplicationException( Response.status( Response.Status.NOT_ACCEPTABLE ).entity( "Cannot represent \"" + type + "\" as html" ).build() );
		 }

		 public override IList<object> ReadList( string input )
		 {
			  throw new WebApplicationException( Response.status( Response.Status.UNSUPPORTED_MEDIA_TYPE ).entity( "Cannot read html" ).build() );
		 }

		 public override IDictionary<string, object> ReadMap( string input, params string[] requiredKeys )
		 {
			  throw new WebApplicationException( Response.status( Response.Status.UNSUPPORTED_MEDIA_TYPE ).entity( "Cannot read html" ).build() );
		 }

		 public override URI ReadUri( string input )
		 {
			  throw new WebApplicationException( Response.status( Response.Status.UNSUPPORTED_MEDIA_TYPE ).entity( "Cannot read html" ).build() );
		 }

		 public override object ReadValue( string input )
		 {
			  throw new WebApplicationException( Response.status( Response.Status.UNSUPPORTED_MEDIA_TYPE ).entity( "Cannot read html" ).build() );
		 }
	}

}