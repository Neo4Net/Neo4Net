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
namespace Neo4Net.Graphdb.impl.notification
{
	using Test = org.junit.jupiter.api.Test;



//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.impl.notification.NotificationCode.CARTESIAN_PRODUCT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.impl.notification.NotificationCode.DEPRECATED_PROCEDURE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.impl.notification.NotificationCode.INDEX_HINT_UNFULFILLABLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.impl.notification.NotificationCode.JOIN_HINT_UNFULFILLABLE;

	internal class NotificationCodeTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConstructNotificationFor_INDEX_HINT_UNFULFILLABLE()
		 internal virtual void ShouldConstructNotificationForINDEXHINTUNFULFILLABLE()
		 {
			  NotificationDetail indexDetail = NotificationDetail_Factory.Index( "Person", "name" );
			  Notification notification = INDEX_HINT_UNFULFILLABLE.notification( InputPosition.empty, indexDetail );

			  assertThat( notification.Title, equalTo( "The request (directly or indirectly) referred to an index that does not exist." ) );
			  assertThat( notification.Severity, equalTo( SeverityLevel.WARNING ) );
			  assertThat( notification.Code, equalTo( "Neo.ClientError.Schema.IndexNotFound" ) );
			  assertThat( notification.Position, equalTo( InputPosition.empty ) );
			  assertThat( notification.Description, equalTo( "The hinted index does not exist, please check the schema (hinted index is: index on :Person(name))" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConstructNotificationFor_CARTESIAN_PRODUCT()
		 internal virtual void ShouldConstructNotificationForCARTESIANPRODUCT()
		 {
			  ISet<string> idents = new SortedSet<string>();
			  idents.Add( "n" );
			  idents.Add( "node2" );
			  NotificationDetail identifierDetail = NotificationDetail_Factory.CartesianProduct( idents );
			  Notification notification = CARTESIAN_PRODUCT.notification( InputPosition.empty, identifierDetail );

			  assertThat( notification.Title, equalTo( "This query builds a cartesian product between disconnected patterns." ) );
			  assertThat( notification.Severity, equalTo( SeverityLevel.WARNING ) );
			  assertThat( notification.Code, equalTo( "Neo.ClientNotification.Statement.CartesianProductWarning" ) );
			  assertThat( notification.Position, equalTo( InputPosition.empty ) );
			  assertThat( notification.Description, equalTo( "If a part of a query contains multiple disconnected patterns, this will build a cartesian product " + "between all those parts. This may produce a large amount of data and slow down query processing. While " + "occasionally intended, it may often be possible to reformulate the query that avoids the use of this cross " + "product, perhaps by adding a relationship between the different parts or by using OPTIONAL MATCH " + "(identifiers are: (n, node2))" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConstructNotificationsFor_JOIN_HINT_UNFULFILLABLE()
		 internal virtual void ShouldConstructNotificationsForJOINHINTUNFULFILLABLE()
		 {
			  IList<string> idents = new List<string>();
			  idents.Add( "n" );
			  idents.Add( "node2" );
			  NotificationDetail identifierDetail = NotificationDetail_Factory.JoinKey( idents );
			  Notification notification = JOIN_HINT_UNFULFILLABLE.notification( InputPosition.empty, identifierDetail );

			  assertThat( notification.Title, equalTo( "The database was unable to plan a hinted join." ) );
			  assertThat( notification.Severity, equalTo( SeverityLevel.WARNING ) );
			  assertThat( notification.Code, equalTo( "Neo.ClientNotification.Statement.JoinHintUnfulfillableWarning" ) );
			  assertThat( notification.Position, equalTo( InputPosition.empty ) );
			  assertThat( notification.Description, equalTo( "The hinted join was not planned. This could happen because no generated plan contained the join key, " + "please try using a different join key or restructure your query. " + "(hinted join key identifiers are: n, node2)" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConstructNotificationsFor_DEPRECATED_PROCEDURE()
		 internal virtual void ShouldConstructNotificationsForDEPRECATEDPROCEDURE()
		 {
			  NotificationDetail identifierDetail = NotificationDetail_Factory.DeprecatedName( "oldName", "newName" );
			  Notification notification = DEPRECATED_PROCEDURE.notification( InputPosition.empty, identifierDetail );

			  assertThat( notification.Title, equalTo( "This feature is deprecated and will be removed in future versions." ) );
			  assertThat( notification.Severity, equalTo( SeverityLevel.WARNING ) );
			  assertThat( notification.Code, equalTo( "Neo.ClientNotification.Statement.FeatureDeprecationWarning" ) );
			  assertThat( notification.Position, equalTo( InputPosition.empty ) );
			  assertThat( notification.Description, equalTo( "The query used a deprecated procedure. ('oldName' has been replaced by 'newName')" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConstructNotificationsFor_DEPRECATED_PROCEDURE_with_no_newName()
		 internal virtual void ShouldConstructNotificationsForDEPRECATEDPROCEDUREWithNoNewName()
		 {
			  NotificationDetail identifierDetail = NotificationDetail_Factory.DeprecatedName( "oldName", "" );
			  Notification notification = DEPRECATED_PROCEDURE.notification( InputPosition.empty, identifierDetail );

			  assertThat( notification.Title, equalTo( "This feature is deprecated and will be removed in future versions." ) );
			  assertThat( notification.Severity, equalTo( SeverityLevel.WARNING ) );
			  assertThat( notification.Code, equalTo( "Neo.ClientNotification.Statement.FeatureDeprecationWarning" ) );
			  assertThat( notification.Position, equalTo( InputPosition.empty ) );
			  assertThat( notification.Description, equalTo( "The query used a deprecated procedure. ('oldName' is no longer supported)" ) );
		 }
	}

}