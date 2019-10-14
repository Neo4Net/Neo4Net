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
namespace Neo4Net.causalclustering.helper
{
	using Assert = org.junit.Assert;
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class ErrorHandlerTest
	{
		 private const string FAILMESSAGE = "More fail";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExecuteAllFailingOperations()
		 public virtual void ShouldExecuteAllFailingOperations()
		 {
			  AtomicBoolean @bool = new AtomicBoolean( false );
			  try
			  {
					ErrorHandler.RunAll("test", Assert.fail, () =>
					{
					 @bool.set( true );
					 throw new System.InvalidOperationException( FAILMESSAGE );
					});
					fail();
			  }
			  catch ( Exception e )
			  {
					assertEquals( "test", e.Message );
					Exception cause = e.InnerException;
					assertEquals( typeof( AssertionError ), cause.GetType() );
					Exception[] suppressed = e.Suppressed;
					assertEquals( 1, suppressed.Length );
					assertEquals( typeof( System.InvalidOperationException ), suppressed[0].GetType() );
					assertEquals( "More fail", suppressed[0].Message );
					assertTrue( @bool.get() );
			  }
		 }
	}

}