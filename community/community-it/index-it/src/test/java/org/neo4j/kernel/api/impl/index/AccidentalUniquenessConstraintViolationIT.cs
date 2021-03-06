﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Api.Impl.Index
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using NotFoundException = Org.Neo4j.Graphdb.NotFoundException;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.ArrayUtil.array;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class AccidentalUniquenessConstraintViolationIT
	public class AccidentalUniquenessConstraintViolationIT
	{
		 private static readonly Label _foo = Label.label( "Foo" );
		 private const string BAR = "bar";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters public static java.util.Collection<Object[]> data()
		 public static ICollection<object[]> Data()
		 {
			  ICollection<object[]> data = new List<object[]>();
			  data.Add( array( 42, 41 ) );
			  data.Add( array( "a", "b" ) );
			  return data;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public Object value1;
		 public object Value1;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public Object value2;
		 public object Value2;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule Db = new ImpermanentDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyChangesWithIntermediateConstraintViolations() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyChangesWithIntermediateConstraintViolations()
		 {
			  // given
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().constraintFor(_foo).assertPropertyIsUnique(BAR).create();
					tx.Success();
			  }
			  Node fourtyTwo;
			  Node fourtyOne;
			  using ( Transaction tx = Db.beginTx() )
			  {
					fourtyTwo = Db.createNode( _foo );
					fourtyTwo.SetProperty( BAR, Value1 );
					fourtyOne = Db.createNode( _foo );
					fourtyOne.SetProperty( BAR, Value2 );
					tx.Success();
			  }

			  // when
			  using ( Transaction tx = Db.beginTx() )
			  {
					fourtyOne.Delete();
					fourtyTwo.SetProperty( BAR, Value2 );
					tx.Success();
			  }

			  // then
			  using ( Transaction tx = Db.beginTx() )
			  {
					assertEquals( Value2, fourtyTwo.GetProperty( BAR ) );
					try
					{
						 fourtyOne.GetProperty( BAR );
						 fail( "Should be deleted" );
					}
					catch ( NotFoundException )
					{
						 // good
					}
					tx.Success();

					assertEquals( fourtyTwo, Db.findNode( _foo, BAR, Value2 ) );
					assertNull( Db.findNode( _foo, BAR, Value1 ) );
			  }
		 }
	}

}