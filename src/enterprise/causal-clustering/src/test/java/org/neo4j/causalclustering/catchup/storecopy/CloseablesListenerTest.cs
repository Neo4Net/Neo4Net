using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.catchup.storecopy
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class CloseablesListenerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public readonly ExpectedException ExpectedException = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseAllReourcesBeforeException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseAllReourcesBeforeException()
		 {
			  // given
			  CloseablesListener closeablesListener = new CloseablesListener();
			  Exception exception = new Exception( "fail" );
			  CloseTrackingCloseable kindCloseable1 = new CloseTrackingCloseable( this );
			  CloseTrackingCloseable unkindCloseable = new CloseTrackingCloseable( this, exception );
			  CloseTrackingCloseable kindCloseable2 = new CloseTrackingCloseable( this );
			  closeablesListener.Add( kindCloseable1 );
			  closeablesListener.Add( unkindCloseable );
			  closeablesListener.Add( kindCloseable2 );

			  //then we expect an exception
			  ExpectedException.expect( exception.GetType() );

			  // when
			  closeablesListener.Close();

			  //then we expect all have closed
			  assertTrue( kindCloseable1.WasClosed );
			  assertTrue( unkindCloseable.WasClosed );
			  assertTrue( kindCloseable2.WasClosed );
		 }

		 internal class CloseTrackingCloseable : IDisposable
		 {
			 private readonly CloseablesListenerTest _outerInstance;

			  internal readonly Exception ThrowOnClose;

			  internal CloseTrackingCloseable( CloseablesListenerTest outerInstance ) : this( outerInstance, null )
			  {
				  this._outerInstance = outerInstance;
			  }

			  internal CloseTrackingCloseable( CloseablesListenerTest outerInstance, Exception throwOnClose )
			  {
				  this._outerInstance = outerInstance;
					this.ThrowOnClose = throwOnClose;
			  }

			  internal bool WasClosed;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
			  public override void Close()
			  {
					WasClosed = true;
					if ( ThrowOnClose != null )
					{
						 throw ThrowOnClose;
					}
			  }
		 }
	}

}