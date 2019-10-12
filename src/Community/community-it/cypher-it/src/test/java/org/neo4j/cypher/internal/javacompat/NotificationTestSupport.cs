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
namespace Neo4Net.Cypher.@internal.javacompat
{
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using Matchers = org.hamcrest.Matchers;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;
	using Rule = org.junit.Rule;
	using ExpectedException = org.junit.rules.ExpectedException;


	using InputPosition = Neo4Net.Graphdb.InputPosition;
	using Notification = Neo4Net.Graphdb.Notification;
	using Result = Neo4Net.Graphdb.Result;
	using SeverityLevel = Neo4Net.Graphdb.SeverityLevel;
	using NotificationCode = Neo4Net.Graphdb.impl.notification.NotificationCode;
	using NotificationDetail = Neo4Net.Graphdb.impl.notification.NotificationDetail;
	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using Procedure = Neo4Net.Procedure.Procedure;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class NotificationTestSupport
	{
		private bool InstanceFieldsInitialized = false;

		public NotificationTestSupport()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RulePlannerUnavailable = Notification( "Neo.ClientNotification.Statement.PlannerUnavailableWarning", containsString( "Using RULE planner is unsupported for current CYPHER version, the query has been executed by an older CYPHER version" ), any( typeof( InputPosition ) ), SeverityLevel.WARNING );
			CartesianProductWarning = Notification( "Neo.ClientNotification.Statement.CartesianProductWarning", containsString( "If a part of a query contains multiple disconnected patterns, this will build a " + "cartesian product between all those parts. This may produce a large amount of data and slow down" + " query processing. " + "While occasionally intended, it may often be possible to reformulate the query that avoids the " + "use of this cross " + "product, perhaps by adding a relationship between the different parts or by using OPTIONAL MATCH" ), any( typeof( InputPosition ) ), SeverityLevel.WARNING );
			LargeLabelCSVWarning = Notification( "Neo.ClientNotification.Statement.NoApplicableIndexWarning", containsString( "Using LOAD CSV with a large data set in a query where the execution plan contains the " + "Using LOAD CSV followed by a MATCH or MERGE that matches a non-indexed label will most likely " + "not perform well on large data sets. Please consider using a schema index." ), any( typeof( InputPosition ) ), SeverityLevel.WARNING );
			EagerOperatorWarning = Notification( "Neo.ClientNotification.Statement.EagerOperatorWarning", containsString( "Using LOAD CSV with a large data set in a query where the execution plan contains the " + "Eager operator could potentially consume a lot of memory and is likely to not perform well. " + "See the Neo4j Manual entry on the Eager operator for more information and hints on " + "how problems could be avoided." ), any( typeof( InputPosition ) ), SeverityLevel.WARNING );
			UnknownPropertyKeyWarning = Notification( "Neo.ClientNotification.Statement.UnknownPropertyKeyWarning", containsString( "the missing property name is" ), any( typeof( InputPosition ) ), SeverityLevel.WARNING );
			UnknownRelationshipWarning = Notification( "Neo.ClientNotification.Statement.UnknownRelationshipTypeWarning", containsString( "the missing relationship type is" ), any( typeof( InputPosition ) ), SeverityLevel.WARNING );
			UnknownLabelWarning = Notification( "Neo.ClientNotification.Statement.UnknownLabelWarning", containsString( "the missing label name is" ), any( typeof( InputPosition ) ), SeverityLevel.WARNING );
			DynamicPropertyWarning = Notification( "Neo.ClientNotification.Statement.DynamicPropertyWarning", containsString( "Using a dynamic property makes it impossible to use an index lookup for this query" ), any( typeof( InputPosition ) ), SeverityLevel.WARNING );
			JoinHintUnsupportedWarning = Notification( "Neo.Status.Statement.JoinHintUnsupportedWarning", containsString( "Using RULE planner is unsupported for queries with join hints, please use COST planner instead" ), any( typeof( InputPosition ) ), SeverityLevel.WARNING );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.ImpermanentDatabaseRule rule = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public readonly ImpermanentDatabaseRule Rule = new ImpermanentDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException thrown = org.junit.rules.ExpectedException.none();
		 public ExpectedException Thrown = ExpectedException.none();

		 protected internal virtual void AssertNotifications( string query, Matcher<IEnumerable<Notification>> matchesExpectation )
		 {
			  using ( Result result = Db().execute(query) )
			  {
					assertThat( result.Notifications, matchesExpectation );
			  }
		 }

		 protected internal virtual Matcher<Notification> Notification( string code, Matcher<string> description, Matcher<InputPosition> position, SeverityLevel severity )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass( this, code, description, position, severity );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<Notification>
		 {
			 private readonly NotificationTestSupport _outerInstance;

			 private string _code;
			 private Matcher<string> _description;
			 private Matcher<InputPosition> _position;
			 private SeverityLevel _severity;

			 public TypeSafeMatcherAnonymousInnerClass( NotificationTestSupport outerInstance, string code, Matcher<string> description, Matcher<InputPosition> position, SeverityLevel severity )
			 {
				 this.outerInstance = outerInstance;
				 this._code = code;
				 this._description = description;
				 this._position = position;
				 this._severity = severity;
			 }

			 protected internal override bool matchesSafely( Notification item )
			 {
				  return _code.Equals( item.Code ) && _description.matches( item.Description ) && _position.matches( item.Position ) && _severity.Equals( item.Severity );
			 }

			 public override void describeTo( Description target )
			 {
				  target.appendText( "Notification{code=" ).appendValue( _code ).appendText( ", description=[" ).appendDescriptionOf( _description ).appendText( "], position=[" ).appendDescriptionOf( _position ).appendText( "], severity=" ).appendValue( _severity ).appendText( "}" );
			 }
		 }

		 protected internal virtual GraphDatabaseAPI Db()
		 {
			  return Rule.GraphDatabaseAPI;
		 }

		 internal virtual Matcher<IEnumerable<Notification>> ContainsNotification( NotificationCode.Notification expected )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass2( this, expected );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass2 : TypeSafeMatcher<IEnumerable<Notification>>
		 {
			 private readonly NotificationTestSupport _outerInstance;

			 private NotificationCode.Notification _expected;

			 public TypeSafeMatcherAnonymousInnerClass2( NotificationTestSupport outerInstance, NotificationCode.Notification expected )
			 {
				 this.outerInstance = outerInstance;
				 this._expected = expected;
			 }

			 protected internal override bool matchesSafely( IEnumerable<Notification> items )
			 {
				  foreach ( Notification item in items )
				  {
						if ( item.Equals( _expected ) )
						{
							 return true;
						}
				  }
				  return false;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "an iterable containing " + _expected );
			 }
		 }

		 internal virtual Matcher<IEnumerable<T>> ContainsItem<T>( Matcher<T> itemMatcher )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass3( this, itemMatcher );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass3 : TypeSafeMatcher<IEnumerable<T>>
		 {
			 private readonly NotificationTestSupport _outerInstance;

			 private Matcher<T> _itemMatcher;

			 public TypeSafeMatcherAnonymousInnerClass3( NotificationTestSupport outerInstance, Matcher<T> itemMatcher )
			 {
				 this.outerInstance = outerInstance;
				 this._itemMatcher = itemMatcher;
			 }

			 protected internal override bool matchesSafely( IEnumerable<T> items )
			 {
				  foreach ( T item in items )
				  {
						if ( _itemMatcher.matches( item ) )
						{
							 return true;
						}
				  }
				  return false;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "an iterable containing " ).appendDescriptionOf( _itemMatcher );
			 }
		 }

		 internal virtual Matcher<IEnumerable<T>> ContainsNoItem<T>( Matcher<T> itemMatcher )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass4( this, itemMatcher );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass4 : TypeSafeMatcher<IEnumerable<T>>
		 {
			 private readonly NotificationTestSupport _outerInstance;

			 private Matcher<T> _itemMatcher;

			 public TypeSafeMatcherAnonymousInnerClass4( NotificationTestSupport outerInstance, Matcher<T> itemMatcher )
			 {
				 this.outerInstance = outerInstance;
				 this._itemMatcher = itemMatcher;
			 }

			 protected internal override bool matchesSafely( IEnumerable<T> items )
			 {
				  foreach ( T item in items )
				  {
						if ( _itemMatcher.matches( item ) )
						{
							 return false;
						}
				  }
				  return true;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "an iterable not containing " ).appendDescriptionOf( _itemMatcher );
			 }
		 }

		 internal virtual void ShouldNotifyInStream( string version, string query, InputPosition pos, NotificationCode code )
		 {
			  //when
			  Result result = Db().execute(version + query);

			  //then
			  NotificationCode.Notification notification = code.notification( pos );
			  assertThat( Iterables.asList( result.Notifications ), Matchers.hasItems( notification ) );
			  IDictionary<string, object> arguments = result.ExecutionPlanDescription.Arguments;
			  assertThat( arguments["version"], equalTo( version ) );
			  result.Close();
		 }

		 internal virtual void ShouldNotifyInStreamWithDetail( string version, string query, InputPosition pos, NotificationCode code, NotificationDetail detail )
		 {
			  //when
			  Result result = Db().execute(version + query);

			  //then
			  NotificationCode.Notification notification = code.notification( pos, detail );
			  assertThat( Iterables.asList( result.Notifications ), Matchers.hasItems( notification ) );
			  IDictionary<string, object> arguments = result.ExecutionPlanDescription.Arguments;
			  assertThat( arguments["version"], equalTo( version ) );
			  result.Close();
		 }

		 internal virtual void ShouldNotNotifyInStream( string version, string query )
		 {
			  // when
			  Result result = Db().execute(version + query);

			  // then
			  assertThat( Iterables.asList( result.Notifications ), empty() );
			  IDictionary<string, object> arguments = result.ExecutionPlanDescription.Arguments;
			  assertThat( arguments["version"], equalTo( version ) );
			  result.Close();
		 }

		 internal Matcher<Notification> RulePlannerUnavailable;

		 internal Matcher<Notification> CartesianProductWarning;

		 internal Matcher<Notification> LargeLabelCSVWarning;

		 internal Matcher<Notification> EagerOperatorWarning;

		 internal Matcher<Notification> UnknownPropertyKeyWarning;

		 internal Matcher<Notification> UnknownRelationshipWarning;

		 internal Matcher<Notification> UnknownLabelWarning;

		 internal Matcher<Notification> DynamicPropertyWarning;

		 internal Matcher<Notification> JoinHintUnsupportedWarning;
	}

}