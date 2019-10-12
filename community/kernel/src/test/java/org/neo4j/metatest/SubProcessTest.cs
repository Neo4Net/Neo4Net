using System;
using System.Threading;

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
namespace Org.Neo4j.Metatest
{
	using Test = org.junit.jupiter.api.Test;

	using Org.Neo4j.Test.subprocess;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	internal class SubProcessTest
	{
		 private const string MESSAGE = "message";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void canInvokeSubprocessMethod() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CanInvokeSubprocessMethod()
		 {
			  Callable<string> subprocess = ( new TestingProcess() ).Start(MESSAGE);
			  try
			  {
					assertEquals( MESSAGE, subprocess.call() );
			  }
			  finally
			  {
					SubProcess.stop( subprocess );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") private static class TestingProcess extends org.neo4j.test.subprocess.SubProcess<java.util.concurrent.Callable<String>, String> implements java.util.concurrent.Callable<String>
		 [Serializable]
		 private class TestingProcess : SubProcess<Callable<string>, string>, Callable<string>
		 {
			  internal string Message;
			  [NonSerialized]
			  internal volatile bool Started;

			  protected internal override void Startup( string parameter )
			  {
					Message = parameter;
					Started = true;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String call() throws Exception
			  public override string Call()
			  {
					while ( !Started )
					{
					// because all calls are asynchronous
						 Thread.Sleep( 1 );
					}
					return Message;
			  }
		 }
	}

}