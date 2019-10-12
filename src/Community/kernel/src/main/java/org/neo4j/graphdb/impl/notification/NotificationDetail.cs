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
namespace Neo4Net.Graphdb.impl.notification
{

	public interface NotificationDetail
	{
		 string Name();

		 string Value();
	}

	 public sealed class NotificationDetail_Factory
	 {
		  internal NotificationDetail_Factory()
		  {
		  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static NotificationDetail deprecatedName(final String oldName, final String newName)
		  public static NotificationDetail DeprecatedName( string oldName, string newName )
		  {
				return CreateDeprecationNotificationDetail( oldName, newName );
		  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static NotificationDetail index(final String labelName, final String... propertyKeyNames)
		  public static NotificationDetail Index( string labelName, params string[] propertyKeyNames )
		  {
				return createNotificationDetail( "hinted index", string.Format( "index on :{0}({1})", labelName, string.join( ",", propertyKeyNames ) ), true );
		  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static NotificationDetail suboptimalIndex(final String labelName, final String... propertyKeyNames)
		  public static NotificationDetail SuboptimalIndex( string labelName, params string[] propertyKeyNames )
		  {
				return createNotificationDetail( "index", string.Format( "index on :{0}({1})", labelName, string.join( ",", propertyKeyNames ) ), true );
		  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static NotificationDetail label(final String labelName)
		  public static NotificationDetail Label( string labelName )
		  {
				return CreateNotificationDetail( "the missing label name", labelName, true );
		  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static NotificationDetail relationshipType(final String relType)
		  public static NotificationDetail RelationshipType( string relType )
		  {
				return CreateNotificationDetail( "the missing relationship type", relType, true );
		  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static NotificationDetail procedureWarning(final String procedure, final String warning)
		  public static NotificationDetail ProcedureWarning( string procedure, string warning )
		  {
				return CreateProcedureWarningNotificationDetail( procedure, warning );
		  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static NotificationDetail propertyName(final String name)
		  public static NotificationDetail PropertyName( string name )
		  {
				return CreateNotificationDetail( "the missing property name", name, true );
		  }

		  public static NotificationDetail JoinKey( IList<string> identifiers )
		  {
				bool singular = identifiers.Count == 1;
				StringBuilder builder = new StringBuilder();
				bool first = true;
				foreach ( string identifier in identifiers )
				{
					 if ( first )
					 {
						  first = false;
					 }
					 else
					 {
						  builder.Append( ", " );
					 }
					 builder.Append( identifier );
				}
				return CreateNotificationDetail( singular ? "hinted join key identifier" : "hinted join key identifiers", builder.ToString(), singular );
		  }

		  public static NotificationDetail CartesianProduct( ISet<string> identifiers )
		  {
				return CreateNotificationDetail( identifiers, "identifier", "identifiers" );
		  }

		  public static NotificationDetail IndexSeekOrScan( ISet<string> labels )
		  {
				return CreateNotificationDetail( labels, "indexed label", "indexed labels" );
		  }

		  public static NotificationDetail Message( string name, string message )
		  {
				return new NotificationDetailAnonymousInnerClass( name, message );
		  }

		  private class NotificationDetailAnonymousInnerClass : NotificationDetail
		  {
			  private string _name;
			  private string _message;

			  public NotificationDetailAnonymousInnerClass( string name, string message )
			  {
				  this._name = name;
				  this._message = message;
			  }

			  public string name()
			  {
					return _name;
			  }

			  public string value()
			  {
					return _message;
			  }

			  public override string ToString()
			  {
					return _message;
			  }
		  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static NotificationDetail deprecatedField(final String procedure, final String field)
		  public static NotificationDetail DeprecatedField( string procedure, string field )
		  {
				return new NotificationDetailAnonymousInnerClass2( procedure, field );
		  }

		  private class NotificationDetailAnonymousInnerClass2 : NotificationDetail
		  {
			  private string _procedure;
			  private string _field;

			  public NotificationDetailAnonymousInnerClass2( string procedure, string field )
			  {
				  this._procedure = procedure;
				  this._field = field;
			  }

			  public string name()
			  {
					return _procedure;
			  }

			  public string value()
			  {
					return _field;
			  }

			  public override string ToString()
			  {
					return string.Format( "'{0}' returned by '{1}' is no longer supported.", _field, _procedure );
			  }
		  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static NotificationDetail bindingVarLengthRelationship(final String element)
		  public static NotificationDetail BindingVarLengthRelationship( string element )
		  {
				return new NotificationDetailAnonymousInnerClass3( element );
		  }

		  private class NotificationDetailAnonymousInnerClass3 : NotificationDetail
		  {
			  private string _element;

			  public NotificationDetailAnonymousInnerClass3( string element )
			  {
				  this._element = element;
			  }

			  public string name()
			  {
					return _element;
			  }

			  public string value()
			  {
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: return String.format("Binding a variable length relationship pattern to a variable ('%s') is deprecated and " + "will be unsupported in a future version. The recommended way is to bind the " + "whole path to a variable, then extract the relationships:%n" + "\tMATCH p = (...)-[...]-(...)%n" + "\tWITH *, relationships(p) AS %s", element, element);
					return string.Format( "Binding a variable length relationship pattern to a variable ('%s') is deprecated and " + "will be unsupported in a future version. The recommended way is to bind the " + "whole path to a variable, then extract the relationships:%n" + "\tMATCH p = (...)-[...]-(...)%n" + "\tWITH *, relationships(p) AS %s", _element, _element );
			  }

			  public override string ToString()
			  {
					return value();
			  }
		  }

		  internal static NotificationDetail CreateNotificationDetail( ISet<string> elements, string singularTerm, string pluralTerm )
		  {
				StringBuilder builder = new StringBuilder();
				builder.Append( '(' );
				string separator = "";
				foreach ( string element in elements )
				{
					 builder.Append( separator );
					 builder.Append( element );
					 separator = ", ";
				}
				builder.Append( ')' );
				bool singular = elements.Count == 1;
				return CreateNotificationDetail( singular ? singularTerm : pluralTerm, builder.ToString(), singular );
		  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: static NotificationDetail createNotificationDetail(final String name, final String value, final boolean singular)
		  internal static NotificationDetail CreateNotificationDetail( string name, string value, bool singular )
		  {
				return new NotificationDetailAnonymousInnerClass4( name, value, singular );
		  }

		  private class NotificationDetailAnonymousInnerClass4 : NotificationDetail
		  {
			  private string _name;
			  private string _value;
			  private bool _singular;

			  public NotificationDetailAnonymousInnerClass4( string name, string value, bool singular )
			  {
				  this._name = name;
				  this._value = value;
				  this._singular = singular;
			  }

			  public string name()
			  {
					return _name;
			  }

			  public string value()
			  {
					return _value;
			  }

			  public override string ToString()
			  {
					return string.Format( "{0} {1} {2}", _name, _singular ? "is:" : "are:", _value );
			  }
		  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: static NotificationDetail createDeprecationNotificationDetail(final String oldName, final String newName)
		  internal static NotificationDetail CreateDeprecationNotificationDetail( string oldName, string newName )
		  {
				return new NotificationDetailAnonymousInnerClass5( oldName, newName );
		  }

		  private class NotificationDetailAnonymousInnerClass5 : NotificationDetail
		  {
			  private string _oldName;
			  private string _newName;

			  public NotificationDetailAnonymousInnerClass5( string oldName, string newName )
			  {
				  this._oldName = oldName;
				  this._newName = newName;
			  }

			  public string name()
			  {
					return _oldName;
			  }

			  public string value()
			  {
					return _newName;
			  }

			  public override string ToString()
			  {
					if ( string.ReferenceEquals( _newName, null ) || _newName.Trim().Length == 0 )
					{
						 return string.Format( "'{0}' is no longer supported", _oldName );
					}
					else
					{
						 return string.Format( "'{0}' has been replaced by '{1}'", _oldName, _newName );
					}
			  }
		  }

		  internal static NotificationDetail CreateProcedureWarningNotificationDetail( string procedure, string warning )
		  {
				return new NotificationDetailAnonymousInnerClass6( procedure, warning );
		  }

		  private class NotificationDetailAnonymousInnerClass6 : NotificationDetail
		  {
			  private string _procedure;
			  private string _warning;

			  public NotificationDetailAnonymousInnerClass6( string procedure, string warning )
			  {
				  this._procedure = procedure;
				  this._warning = warning;
			  }

			  public string name()
			  {
					return _procedure;
			  }

			  public string value()
			  {
					return _warning;
			  }

			  public override string ToString()
			  {
					return string.format( _warning, _procedure );
			  }
		  }
	 }

}