using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.@internal.cypher.acceptance
{

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using ComponentRegistry = Org.Neo4j.Kernel.impl.proc.ComponentRegistry;
	using Context = Org.Neo4j.Procedure.Context;
	using Description = Org.Neo4j.Procedure.Description;
	using Mode = Org.Neo4j.Procedure.Mode;
	using Name = Org.Neo4j.Procedure.Name;
	using Procedure = Org.Neo4j.Procedure.Procedure;
	using UserFunction = Org.Neo4j.Procedure.UserFunction;

	public class TestResourceProcedure
	{
		 internal abstract class SimulateFailureBaseException : Exception
		 {
		 }

		 public class SimulateFailureException : SimulateFailureBaseException
		 {
		 }

		 public class SimulateFailureOnCloseException : SimulateFailureBaseException
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.graphdb.GraphDatabaseService db;
		 public GraphDatabaseService Db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public Counters counters;
		 public Counters Counters;

		 public class Counters
		 {
			  public int CloseCountTestResourceProcedure;
			  public int CloseCountTestFailingResourceProcedure;
			  public int CloseCountTestOnCloseFailingResourceProcedure;

			  public int OpenCountTestResourceProcedure;
			  public int OpenCountTestFailingResourceProcedure;
			  public int OpenCountTestOnCloseFailingResourceProcedure;

			  public virtual int LiveCountTestResourceProcedure()
			  {
					return OpenCountTestResourceProcedure - CloseCountTestResourceProcedure;
			  }

			  public virtual int LiveCountTestFailingResourceProcedure()
			  {
					return OpenCountTestFailingResourceProcedure - CloseCountTestFailingResourceProcedure;
			  }

			  public virtual int LiveCountTestOnCloseFailingResourceProcedure()
			  {
					return OpenCountTestOnCloseFailingResourceProcedure - CloseCountTestOnCloseFailingResourceProcedure;
			  }

			  public virtual void Reset()
			  {
					CloseCountTestResourceProcedure = 0;
					CloseCountTestFailingResourceProcedure = 0;
					CloseCountTestOnCloseFailingResourceProcedure = 0;
					OpenCountTestResourceProcedure = 0;
					OpenCountTestFailingResourceProcedure = 0;
					OpenCountTestOnCloseFailingResourceProcedure = 0;
			  }
		 }

		 public static ComponentRegistry.Provider<Counters> CountersProvider( Counters counters )
		 {
			  return context => counters;
		 }

		 public class Output
		 {
			 private readonly TestResourceProcedure _outerInstance;

			  public long? Value;

			  public Output( TestResourceProcedure outerInstance, long? value )
			  {
				  this._outerInstance = outerInstance;
					this.Value = value;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure(name = "org.neo4j.test.testResourceProcedure", mode = org.neo4j.procedure.Mode.READ) @Description("Returns a stream of integers from 1 to the given argument") public java.util.stream.Stream<Output> testResourceProcedure(@Name(value = "resultCount", defaultValue = "4") long resultCount) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 [Procedure(name : "org.neo4j.test.testResourceProcedure", mode : Org.Neo4j.Procedure.Mode.READ), Description("Returns a stream of integers from 1 to the given argument")]
		 public virtual Stream<Output> TestResourceProcedureConflict( long resultCount )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  Stream<Output> stream = Stream.iterate( 1L, i => i + 1 ).limit( resultCount ).map( Output::new );
			  stream.onClose(() =>
			  {
			  Counters.closeCountTestResourceProcedure++;
			  });
			  Counters.openCountTestResourceProcedure++;
			  return stream;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure(name = "org.neo4j.test.testFailingResourceProcedure", mode = org.neo4j.procedure.Mode.READ) @Description("Returns a stream of integers from 1 to the given argument, but throws an exception when reaching the last element") public java.util.stream.Stream<Output> testFailingResourceProcedure(@Name(value = "failCount", defaultValue = "3") long failCount) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Procedure(name : "org.neo4j.test.testFailingResourceProcedure", mode : Org.Neo4j.Procedure.Mode.READ), Description("Returns a stream of integers from 1 to the given argument, but throws an exception when reaching the last element")]
		 public virtual Stream<Output> TestFailingResourceProcedure( long failCount )
		 {
			  IEnumerator<Output> failingIterator = new IteratorAnonymousInnerClass( this, failCount );
			  IEnumerable<Output> failingIterable = () => failingIterator;
			  Stream<Output> stream = StreamSupport.stream( failingIterable.spliterator(), false );
			  stream.onClose(() =>
			  {
			  Counters.closeCountTestFailingResourceProcedure++;
			  });
			  Counters.openCountTestFailingResourceProcedure++;
			  return stream;
		 }

		 private class IteratorAnonymousInnerClass : IEnumerator<Output>
		 {
			 private readonly TestResourceProcedure _outerInstance;

			 private long _failCount;

			 public IteratorAnonymousInnerClass( TestResourceProcedure outerInstance, long failCount )
			 {
				 this.outerInstance = outerInstance;
				 this._failCount = failCount;
				 step = 1;
			 }

			 private long step;

			 public bool hasNext()
			 {
				  return step <= _failCount;
			 }

			 public Output next()
			 {
				  if ( step == _failCount )
				  {
						throw new SimulateFailureException();
				  }
				  return new Output( _outerInstance, step++ );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure(name = "org.neo4j.test.testOnCloseFailingResourceProcedure", mode = org.neo4j.procedure.Mode.READ) @Description("Returns a stream of integers from 1 to the given argument. Throws an exception on close.") public java.util.stream.Stream<Output> testOnCloseFailingResourceProcedure(@Name(value = "resultCount", defaultValue = "4") long resultCount) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Procedure(name : "org.neo4j.test.testOnCloseFailingResourceProcedure", mode : Org.Neo4j.Procedure.Mode.READ), Description("Returns a stream of integers from 1 to the given argument. Throws an exception on close.")]
		 public virtual Stream<Output> TestOnCloseFailingResourceProcedure( long resultCount )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  Stream<Output> stream = Stream.iterate( 1L, i => i + 1 ).limit( resultCount ).map( Output::new );
			  stream.onClose(() =>
			  {
			  Counters.closeCountTestOnCloseFailingResourceProcedure++;
			  throw new SimulateFailureOnCloseException();
			  });
			  Counters.openCountTestOnCloseFailingResourceProcedure++;
			  return stream;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction(name = "org.neo4j.test.fail") @Description("org.neo4j.test.fail") public String fail(@Name(value = "input") String input)
		 [UserFunction(name : "org.neo4j.test.fail"), Description("org.neo4j.test.fail")]
		 public virtual string Fail( string input )
		 {
			  throw new SimulateFailureException();
		 }
	}

}