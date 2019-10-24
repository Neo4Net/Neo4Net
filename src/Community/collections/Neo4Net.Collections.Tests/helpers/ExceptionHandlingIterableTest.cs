using System;

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
namespace Neo4Net.Collections.Helpers
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") class ExceptionHandlingIterableTest
	internal class ExceptionHandlingIterableTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testHandleExceptionOnIteratorCreation()
		 internal virtual void TestHandleExceptionOnIteratorCreation()
		 {
			  assertThrows(typeof(System.InvalidOperationException), () => Iterables.count(new ExceptionHandlingIterable(() =>
			  {
				throw new Exception( "exception on iterator" );
			  }){@Override protected System.Collections.IEnumerator exceptionOnIterator( Exception t ){rethrow( new System.InvalidOperationException() );
						 return base.exceptionOnIterator( t );
		 }
	}
			 internal ));
}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testHandleExceptionOnNext()
		 internal virtual void testHandleExceptionOnNext()
		 {
			  assertThrows(typeof(System.InvalidOperationException), () => Iterables.count(new ExceptionHandlingIterable(() => new IteratorAnonymousInnerClass(this)){@Override protected object exceptionOnNext(Exception t){rethrow(new System.InvalidOperationException());
									return base.exceptionOnNext( t );
		 }

							  private class IteratorAnonymousInnerClass : System.Collections.IEnumerator
							  {
								  private readonly MissingClass outerInstance;

								  public IteratorAnonymousInnerClass( MissingClass outerInstance )
								  {
									  this.outerInstance = outerInstance;
								  }

								  public bool hasNext()
								  {
										return true;
								  }

								  public object next()
								  {
										throw new Exception( "exception on next" );
								  }

								  public void remove()
								  {
								  }
							  }
						 }
						internal ));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testHandleExceptionOnHasNext()
		 internal virtual void testHandleExceptionOnHasNext()
		 {
			  assertThrows(typeof(System.InvalidOperationException), () => Iterables.count(new ExceptionHandlingIterable(() => new IteratorAnonymousInnerClass2(this)){@Override protected bool exceptionOnHasNext(Exception t){rethrow(new System.InvalidOperationException());
									return base.exceptionOnHasNext( t );
		 }

							  private class IteratorAnonymousInnerClass2 : System.Collections.IEnumerator
							  {
								  private readonly MissingClass outerInstance;

								  public IteratorAnonymousInnerClass2( MissingClass outerInstance )
								  {
									  this.outerInstance = outerInstance;
								  }

								  public bool hasNext()
								  {
										throw new Exception( "exception on next" );
								  }

								  public object next()
								  {
										return null;
								  }

								  public void remove()
								  {
								  }
							  }
						 }
						internal ));
		 }
	}

}