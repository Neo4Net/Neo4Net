using System;
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
namespace Neo4Net.Server.rest.domain
{

	/// <summary>
	/// This is just a simple test of how a HTML renderer could be like
	/// </summary>
	public class HtmlHelper
	{
		 private const string STYLE_LOCATION = "http://resthtml.Neo4Net.org/style/";

		 private HtmlHelper()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static String from(final Object object, final ObjectType objectType)
		 public static string From( object @object, ObjectType objectType )
		 {
			  StringBuilder builder = Start( objectType, null );
			  Append( builder, @object, objectType );
			  return End( builder );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static StringBuilder start(final ObjectType objectType, final String additionalCodeInHead)
		 public static StringBuilder Start( ObjectType objectType, string additionalCodeInHead )
		 {
			  return start( objectType.Caption, additionalCodeInHead );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static StringBuilder start(final String title, final String additionalCodeInHead)
		 public static StringBuilder Start( string title, string additionalCodeInHead )
		 {
			  StringBuilder builder = new StringBuilder();
			  builder.Append( "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\">\n" );
			  builder.Append( "<html><head><title>" + title + "</title>" );
			  if ( !string.ReferenceEquals( additionalCodeInHead, null ) )
			  {
					builder.Append( additionalCodeInHead );
			  }
			  builder.Append( "<meta content=\"text/html; charset=utf-8\" http-equiv=\"Content-Type\">\n" + "<link href='" + STYLE_LOCATION + "rest.css' rel='stylesheet' type='text/css'>\n" + "</head>\n<body onload='javascript:Neo4NetHtmlBrowse.start();' id='" + title.ToLower() + "'>\n" + "<div id='content'>" + "<div id='header'>" + "<h1><a title='Neo4Net REST interface' href='/'><span>Neo4Net REST interface</span></a></h1>" + "</div>" + "\n<div id='page-body'>\n" );
			  return builder;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static String end(final StringBuilder builder)
		 public static string End( StringBuilder builder )
		 {
			  builder.Append( "<div class='break'>&nbsp;</div>" + "</div></div></body></html>" );
			  return builder.ToString();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static void appendMessage(final StringBuilder builder, final String message)
		 public static void AppendMessage( StringBuilder builder, string message )
		 {
			  builder.Append( "<p class=\"message\">" + message + "</p>" );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static void append(final StringBuilder builder, final Object object, final ObjectType objectType)
		 public static void Append( StringBuilder builder, object @object, ObjectType objectType )
		 {
			  if ( @object is System.Collections.ICollection )
			  {
					builder.Append( "<ul>\n" );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (Object item : (java.util.Collection<?>) object)
					foreach ( object item in ( ICollection<object> ) @object )
					{
						 builder.Append( "<li>" );
						 Append( builder, item, objectType );
						 builder.Append( "</li>\n" );
					}
					builder.Append( "</ul>\n" );
			  }
			  else if ( @object is System.Collections.IDictionary )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<?, ?> map = (java.util.Map<?, ?>) object;
					IDictionary<object, ?> map = ( IDictionary<object, ?> ) @object;
					string htmlClass = objectType.HtmlClass;
					string caption = objectType.Caption;
					if ( map.Count > 0 )
					{
						 bool isNodeOrRelationship = ObjectType.Node.Equals( objectType ) || ObjectType.Relationship.Equals( objectType );
						 if ( isNodeOrRelationship )
						 {
							  builder.Append( "<h2>" + caption + "</h2>\n" );
							  Append( builder, map["data"], ObjectType.Properties );
							  htmlClass = "meta";
							  caption += " info";
						 }
						 if ( ObjectType.Node.Equals( objectType ) && map.Count == 1 )
						 {
							  // there's only properties, so we're finished here
							  return;
						 }
						 builder.Append( "<table class=\"" + htmlClass + "\"><caption>" );
						 builder.Append( caption );
						 builder.Append( "</caption>\n" );
						 bool odd = true;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.Map.Entry<?, ?> entry : map.entrySet())
						 foreach ( KeyValuePair<object, ?> entry in map.SetOfKeyValuePairs() )
						 {
							  if ( isNodeOrRelationship && "data".Equals( entry.Key ) )
							  {
									continue;
							  }
							  builder.Append( "<tr" + ( odd ? " class='odd'" : "" ) + ">" );
							  odd = !odd;
							  builder.Append( "<th>" + entry.Key + "</th><td>" );
							  // TODO We always assume that an inner map is for
							  // properties, correct?
							  Append( builder, entry.Value, ObjectType.Properties );
							  builder.Append( "</td></tr>\n" );
						 }
						 builder.Append( "</table>\n" );
					}
					else
					{
						 builder.Append( "<table class=\"" + htmlClass + "\"><caption>" );
						 builder.Append( caption );
						 builder.Append( "</caption>" );
						 builder.Append( "<tr><td></td></tr>" );
						 builder.Append( "</table>" );
					}
			  }
			  else
			  {
					builder.Append( @object != null ? EmbedInLinkIfClickable( @object.ToString() ) : "" );
			  }
		 }

		 private static string EmbedInLinkIfClickable( string @string )
		 {
			  // TODO Hardcode "http://" string?
			  if ( @string.StartsWith( "http://", StringComparison.Ordinal ) || @string.StartsWith( "https://", StringComparison.Ordinal ) )
			  {
					string anchoredString = "<a href=\"" + @string + "\"";

					// TODO Hardcoded /node/, /relationship/ string?
					string anchorClass = null;
					if ( @string.Contains( "/node/" ) )
					{
						 anchorClass = "node";
					}
					else if ( @string.Contains( "/relationship/" ) )
					{
						 anchorClass = "relationship";
					}
					if ( !string.ReferenceEquals( anchorClass, null ) )
					{
						 anchoredString += " class=\"" + anchorClass + "\"";
					}
					anchoredString += ">" + EscapeHtml( @string ) + "</a>";
					@string = anchoredString;
			  }
			  else
			  {
					@string = EscapeHtml( @string );
			  }
			  return @string;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static String escapeHtml(final String string)
		 private static string EscapeHtml( string @string )
		 {
			  if ( string.ReferenceEquals( @string, null ) )
			  {
					return null;
			  }
			  string res = @string.Replace( "&", "&amp;" );
			  res = res.Replace( "\"", "&quot;" );
			  res = res.Replace( "<", "&lt;" );
			  res = res.Replace( ">", "&gt;" );
			  return res;
		 }

		 public sealed class ObjectType
		 {
			  public static readonly ObjectType Node = new ObjectType( "Node", InnerEnum.Node );
			  public static readonly ObjectType Relationship = new ObjectType( "Relationship", InnerEnum.Relationship );
			  public static readonly ObjectType Properties = new ObjectType( "Properties", InnerEnum.Properties );
			  public static readonly ObjectType Root = new ObjectType( "Root", InnerEnum.Root );
			  public static readonly ObjectType IndexRoot = new ObjectType( "IndexRoot", InnerEnum.IndexRoot );

			  private static readonly IList<ObjectType> valueList = new List<ObjectType>();

			  static ObjectType()
			  {
				  valueList.Add( Node );
				  valueList.Add( Relationship );
				  valueList.Add( Properties );
				  valueList.Add( Root );
				  valueList.Add( IndexRoot );
			  }

			  public enum InnerEnum
			  {
				  Node,
				  Relationship,
				  Properties,
				  Root,
				  IndexRoot
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private ObjectType( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal string Caption
			  {
				  get
				  {
						return name().substring(0, 1).ToUpper() + name().substring(1).ToLower();
				  }
			  }

			  internal string HtmlClass
			  {
				  get
				  {
						return Caption.ToLower();
				  }
			  }

			 public static IList<ObjectType> values()
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

			 public static ObjectType valueOf( string name )
			 {
				 foreach ( ObjectType enumInstance in ObjectType.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }
	}

}