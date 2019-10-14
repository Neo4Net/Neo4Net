using System.Diagnostics;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Neo4Net.Cypher.@internal.v3_5.util
{
	/*
	Why is this here!?
	
	assert in Scala does something different to assert in Java. In Scala, it's controlled through a compiler setting,
	which means you can't use the same binaries and enable/disable assertions through a JVM configuration.
	
	We want the Java behaviour in Scala, and this is how we achieve that.
	 */
	public class AssertionRunner
	{
		 private AssertionRunner()
		 {
			  throw new AssertionError( "No instances" );
		 }

		 public static void RunUnderAssertion( Thunk thunk )
		 {
			  Debug.Assert( RunIt( thunk ) );
		 }

		 private static bool RunIt( Thunk thunk )
		 {
			  thunk.Apply();
			  return true;
		 }

		 public interface Thunk
		 {
			  void Apply();
		 }

		 public static bool AssertionsEnabled
		 {
			 get
			 {
				  bool assertionsEnabled = false;
				  Debug.Assert( assertionsEnabled = true );
				  return assertionsEnabled;
			 }
		 }
	}

}