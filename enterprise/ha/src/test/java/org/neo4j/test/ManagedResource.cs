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
namespace Org.Neo4j.Test
{
	using TestRule = org.junit.rules.TestRule;
	using Description = org.junit.runner.Description;
	using Statement = org.junit.runners.model.Statement;

	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

	public abstract class ManagedResource<R> : TestRule
	{
		 private R _resource;

		 public R Resource
		 {
			 get
			 {
				  R result = this._resource;
				  if ( result == default( R ) )
				  {
						throw new System.InvalidOperationException( "Resource is not started." );
				  }
				  return result;
			 }
		 }

		 protected internal abstract R CreateResource( TestDirectory dir );

		 protected internal abstract void DisposeResource( R resource );

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public final org.junit.runners.model.Statement apply(final org.junit.runners.model.Statement super, org.junit.runner.Description description)
		 public override Statement Apply( Statement @base, Description description )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.rule.TestDirectory dir = org.neo4j.test.rule.TestDirectory.testDirectory(description.getTestClass());
			  TestDirectory dir = TestDirectory.testDirectory( description.TestClass );
			  return dir.apply(new StatementAnonymousInnerClass(this, @base, dir)
			 , description);
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private readonly ManagedResource<R> _outerInstance;

			 private Statement @base;
			 private TestDirectory _dir;

			 public StatementAnonymousInnerClass( ManagedResource<R> outerInstance, Statement @base, TestDirectory dir )
			 {
				 this.outerInstance = outerInstance;
				 this.@base = @base;
				 this._dir = dir;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void evaluate() throws Throwable
			 public override void evaluate()
			 {
				  _outerInstance.resource = _outerInstance.createResource( _dir );
				  try
				  {
						@base.evaluate();
				  }
				  finally
				  {
						R waste = _outerInstance.resource;
						_outerInstance.resource = default( R );
						outerInstance.DisposeResource( waste );
				  }
			 }
		 }
	}

}