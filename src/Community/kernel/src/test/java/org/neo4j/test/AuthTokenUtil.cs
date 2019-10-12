using System;
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
namespace Neo4Net.Test
{
	using BaseMatcher = org.hamcrest.BaseMatcher;
	using Description = org.hamcrest.Description;
	using ArgumentMatcher = org.mockito.ArgumentMatcher;


	using AuthToken = Neo4Net.Kernel.api.security.AuthToken;
	using UTF8 = Neo4Net.@string.UTF8;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.@internal.progress.ThreadSafeMockingProgress.mockingProgress;

	public class AuthTokenUtil
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static boolean matches(java.util.Map<String,Object> expected, Object actualObject)
		 public static bool Matches( IDictionary<string, object> expected, object actualObject )
		 {
			  if ( expected == null || actualObject == null )
			  {
					return expected == actualObject;
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: if (!(actualObject instanceof java.util.Map<?,?>))
			  if ( !( actualObject is IDictionary<object, ?> ) )
			  {
					return false;
			  }

			  IDictionary<string, object> actual = ( IDictionary<string, object> ) actualObject;

			  if ( expected.Count != actual.Count )
			  {
					return false;
			  }

			  foreach ( KeyValuePair<string, object> expectedEntry in expected.SetOfKeyValuePairs() )
			  {
					string key = expectedEntry.Key;
					object expectedValue = expectedEntry.Value;
					object actualValue = actual[key];
					if ( AuthToken.containsSensitiveInformation( key ) )
					{
						 sbyte[] expectedByteArray = expectedValue is sbyte[] ? ( sbyte[] ) expectedValue : expectedValue != null ? UTF8.encode( ( string ) expectedValue ) : null;
						 if ( !Arrays.Equals( expectedByteArray, ( sbyte[] ) actualValue ) )
						 {
							  return false;
						 }
					}
					else if ( expectedValue == null || actualValue == null )
					{
						 return expectedValue == actualValue;
					}
					else if ( !expectedValue.Equals( actualValue ) )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public static void AssertAuthTokenMatches( IDictionary<string, object> expected, IDictionary<string, object> actual )
		 {
			  assertFalse( expected == null ^ actual == null );
			  assertEquals( expected.Keys, actual.Keys );
			  expected.forEach((key, expectedValue) =>
			  {
				object actualValue = actual[key];
				if ( AuthToken.containsSensitiveInformation( key ) )
				{
					 sbyte[] expectedByteArray = expectedValue != null ? UTF8.encode( ( string ) expectedValue ) : null;
					 assertTrue( expectedByteArray.SequenceEqual( ( sbyte[] ) actualValue ) );
				}
				else
				{
					 assertEquals( expectedValue, actualValue );
				}
			  });
		 }

		 public class AuthTokenMatcher : BaseMatcher<IDictionary<string, object>>
		 {
			  internal readonly IDictionary<string, object> ExpectedValue;

			  public AuthTokenMatcher( IDictionary<string, object> expectedValue )
			  {
					this.ExpectedValue = expectedValue;
			  }

			  public override bool Matches( object o )
			  {
					return AuthTokenUtil.Matches( ExpectedValue, o );
			  }

			  public override void DescribeTo( Description description )
			  {
					description.appendValue( this.ExpectedValue );
			  }
		 }

		 public static AuthTokenMatcher AuthTokenMatcher( IDictionary<string, object> authToken )
		 {
			  return new AuthTokenMatcher( authToken );
		 }

		 [Serializable]
		 public class AuthTokenArgumentMatcher : ArgumentMatcher<IDictionary<string, object>>
		 {

			  internal IDictionary<string, object> Wanted;

			  public AuthTokenArgumentMatcher( IDictionary<string, object> authToken )
			  {
					this.Wanted = authToken;
			  }

			  public virtual bool Matches( IDictionary<string, object> actual )
			  {
					return AuthTokenUtil.Matches( Wanted, actual );
			  }

			  public override string ToString()
			  {
					return "authTokenArgumentMatcher(" + Wanted + ")";
			  }
		 }

		 public static IDictionary<string, object> AuthTokenArgumentMatcher( IDictionary<string, object> authToken )
		 {
			  mockingProgress().ArgumentMatcherStorage.reportMatcher(new AuthTokenArgumentMatcher(authToken));
			  return null;
		 }
	}

}