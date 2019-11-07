using System;
using System.Collections.Generic;
using System.Reflection;
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
namespace Neo4Net.GraphDb
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.QueryExecutionType.QueryType.READ_ONLY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.QueryExecutionType.QueryType.READ_WRITE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.QueryExecutionType.QueryType.SCHEMA_WRITE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.QueryExecutionType.QueryType.WRITE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.QueryExecutionType.explained;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.QueryExecutionType.profiled;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.QueryExecutionType.query;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class QueryExecutionTypeTest
	public class QueryExecutionTypeTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.List<Object[]> cases()
		 public static IList<object[]> Cases()
		 {
			  return Arrays.asList( Verify( That( query( READ_ONLY ) ).canContainResults() ), Verify(That(query(READ_WRITE)).canContainResults().canUpdateData()), Verify(That(query(WRITE)).canUpdateData()), Verify(That(query(SCHEMA_WRITE)).canUpdateSchema()), Verify(That(profiled(READ_ONLY)).Explained.Profiled.canContainResults()), Verify(That(profiled(READ_WRITE)).Explained.Profiled.canContainResults().canUpdateData()), Verify(That(profiled(WRITE)).Explained.Profiled.canUpdateData()), Verify(That(profiled(SCHEMA_WRITE)).Explained.Profiled.canUpdateSchema()), Verify(That(explained(READ_ONLY)).Explained.OnlyExplained), Verify(That(explained(READ_WRITE)).Explained.OnlyExplained), Verify(That(explained(WRITE)).Explained.OnlyExplained), Verify(That(explained(SCHEMA_WRITE)).Explained.OnlyExplained), Verify(ThatQueryOf(explained(READ_ONLY)).canContainResults()), Verify(ThatQueryOf(explained(READ_WRITE)).canContainResults().canUpdateData()), Verify(ThatQueryOf(explained(WRITE)).canUpdateData()), Verify(ThatQueryOf(explained(SCHEMA_WRITE)).canUpdateSchema()) );
		 }

		 private readonly Assumptions _expected;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void verify()
		 public virtual void Verify()
		 {
			  QueryExecutionType executionType = _expected.type();
			  assertEquals( _expected.isProfiled, executionType.Profiled );
			  assertEquals( _expected.requestedExecutionPlanDescription, executionType.RequestedExecutionPlanDescription() );
			  assertEquals( _expected.isExplained, executionType.Explained );
			  assertEquals( _expected.canContainResults, executionType.CanContainResults() );
			  assertEquals( _expected.canUpdateData, executionType.CanUpdateData() );
			  assertEquals( _expected.canUpdateSchema, executionType.CanUpdateSchema() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void noneOtherLikeIt()
		 public virtual void NoneOtherLikeIt()
		 {
			  foreach ( QueryExecutionType.QueryType queryType in QueryExecutionType.QueryType.values() )
			  {
					foreach ( QueryExecutionType type in new QueryExecutionType[]{ query( queryType ), profiled( queryType ), explained( queryType ) } )
					{
						 // the very same object will have the same flags, as will all the explained ones...
						 if ( type != _expected.type() && !(_expected.type().Explained && type.Explained) )
						 {
							  assertFalse( _expected.type().ToString(), _expected.isProfiled == type.Profiled && _expected.requestedExecutionPlanDescription == type.RequestedExecutionPlanDescription() && _expected.isExplained == type.Explained && _expected.canContainResults == type.CanContainResults() && _expected.canUpdateData == type.CanUpdateData() && _expected.canUpdateSchema == type.CanUpdateSchema() );
						 }
					}
			  }
		 }

		 public QueryExecutionTypeTest( Assumptions expected )
		 {

			  this._expected = expected;
		 }

		 private static object[] Verify( Assumptions assumptions )
		 {
			  return new object[]{ assumptions };
		 }

		 private static Assumptions That( QueryExecutionType type )
		 {
			  return new Assumptions( type, false );
		 }

		 private static Assumptions ThatQueryOf( QueryExecutionType type )
		 {
			  return new Assumptions( type, true );
		 }

		 internal class Assumptions
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly QueryExecutionType TypeConflict;
			  internal readonly bool ConvertToQuery;
			  internal bool IsProfiled;
			  internal bool RequestedExecutionPlanDescription;
			  internal bool IsExplained;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool CanContainResultsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool CanUpdateDataConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool CanUpdateSchemaConflict;

			  internal Assumptions( QueryExecutionType type, bool convertToQuery )
			  {
					this.TypeConflict = type;
					this.ConvertToQuery = convertToQuery;
			  }

			  public override string ToString()
			  {
					StringBuilder result = new StringBuilder( TypeConflict.ToString() );
					if ( ConvertToQuery )
					{
						 result.Append( " (as query)" );
					}
					string sep = ": ";
					foreach ( System.Reflection.FieldInfo field in this.GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance) )
					{
						 if ( field.Type == typeof( bool ) )
						 {
							  bool value;
							  field.Accessible = true;
							  try
							  {
									value = field.getBoolean( this );
							  }
							  catch ( IllegalAccessException e )
							  {
									throw new Exception( e );
							  }
							  result.Append( sep ).Append( '.' ).Append( field.Name ).Append( "() == " ).Append( value );
							  sep = ", ";
						 }
					}
					return result.ToString();
			  }

			  public virtual Assumptions Profiled
			  {
				  get
				  {
						this.IsProfiled = true;
						return this;
				  }
			  }

			  public virtual Assumptions Explained
			  {
				  get
				  {
						this.RequestedExecutionPlanDescription = true;
						return this;
				  }
			  }

			  public virtual Assumptions OnlyExplained
			  {
				  get
				  {
						this.IsExplained = true;
						return this;
				  }
			  }

			  public virtual Assumptions CanContainResults()
			  {
					this.CanContainResultsConflict = true;
					return this;
			  }

			  public virtual Assumptions CanUpdateData()
			  {
					this.CanUpdateDataConflict = true;
					return this;
			  }

			  public virtual Assumptions CanUpdateSchema()
			  {
					this.CanUpdateSchemaConflict = true;
					return this;
			  }

			  public virtual QueryExecutionType Type()
			  {
					return ConvertToQuery ? query( TypeConflict.queryType() ) : TypeConflict;
			  }
		 }
	}

}