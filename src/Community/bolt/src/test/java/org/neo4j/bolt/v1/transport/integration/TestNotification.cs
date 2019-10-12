using System.Collections.Generic;

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
namespace Neo4Net.Bolt.v1.transport.integration
{

	using InputPosition = Neo4Net.Graphdb.InputPosition;
	using Notification = Neo4Net.Graphdb.Notification;
	using SeverityLevel = Neo4Net.Graphdb.SeverityLevel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasKey;

	public class TestNotification : Notification
	{
		 private readonly string _code;
		 private readonly string _title;
		 private readonly string _description;
		 private readonly SeverityLevel _severityLevel;
		 private readonly InputPosition _position;

		 public TestNotification( string code, string title, string description, SeverityLevel severityLevel, InputPosition position )
		 {
			  this._code = code;
			  this._title = title;
			  this._description = description;
			  this._severityLevel = severityLevel;
			  this._position = position != null ? position : InputPosition.empty;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static org.neo4j.graphdb.Notification fromMap(java.util.Map<String,Object> notification)
		 public static Notification FromMap( IDictionary<string, object> notification )
		 {
			  assertThat( notification, hasKey( "code" ) );
			  assertThat( notification, hasKey( "title" ) );
			  assertThat( notification, hasKey( "description" ) );
			  assertThat( notification, hasKey( "severity" ) );
			  InputPosition position = null;
			  if ( notification.ContainsKey( "position" ) )
			  {
					IDictionary<string, long> pos = ( IDictionary<string, long> ) notification["position"];
					assertThat( pos, hasKey( "offset" ) );
					assertThat( pos, hasKey( "line" ) );
					assertThat( pos, hasKey( "column" ) );
					position = new InputPosition( pos["offset"].intValue(), pos["line"].intValue(), pos["column"].intValue() );
			  }

			  return new TestNotification( ( string ) notification["code"], ( string ) notification["title"], ( string ) notification["description"], Enum.Parse( typeof( SeverityLevel ), ( string ) notification["severity"] ), position );
		 }

		 public virtual string Code
		 {
			 get
			 {
				  return _code;
			 }
		 }

		 public virtual string Title
		 {
			 get
			 {
				  return _title;
			 }
		 }

		 public virtual string Description
		 {
			 get
			 {
				  return _description;
			 }
		 }

		 public virtual SeverityLevel Severity
		 {
			 get
			 {
				  return _severityLevel;
			 }
		 }

		 public virtual InputPosition Position
		 {
			 get
			 {
				  return _position;
			 }
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  TestNotification that = ( TestNotification ) o;

			  if ( !string.ReferenceEquals( _code, null ) ?!_code.Equals( that._code ) :!string.ReferenceEquals( that._code, null ) )
			  {
					return false;
			  }
			  if ( !string.ReferenceEquals( _title, null ) ?!_title.Equals( that._title ) :!string.ReferenceEquals( that._title, null ) )
			  {
					return false;
			  }
			  if ( !string.ReferenceEquals( _description, null ) ?!_description.Equals( that._description ) :!string.ReferenceEquals( that._description, null ) )
			  {
					return false;
			  }
			  if ( _severityLevel != that._severityLevel )
			  {
					return false;
			  }
			  return _position != null ? _position.Equals( that._position ) : that._position == null;

		 }

		 public override int GetHashCode()
		 {
			  int result = !string.ReferenceEquals( _code, null ) ? _code.GetHashCode() : 0;
			  result = 31 * result + ( !string.ReferenceEquals( _title, null ) ? _title.GetHashCode() : 0 );
			  result = 31 * result + ( !string.ReferenceEquals( _description, null ) ? _description.GetHashCode() : 0 );
			  result = 31 * result + ( _severityLevel != null ? _severityLevel.GetHashCode() : 0 );
			  result = 31 * result + ( _position != null ? _position.GetHashCode() : 0 );
			  return result;
		 }
	}

}