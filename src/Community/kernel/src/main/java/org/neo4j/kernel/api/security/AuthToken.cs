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
namespace Neo4Net.Kernel.api.security
{
	using StringUtils = org.apache.commons.lang3.StringUtils;


	using InvalidAuthTokenException = Neo4Net.Kernel.api.security.exception.InvalidAuthTokenException;
	using UTF8 = Neo4Net.@string.UTF8;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;

	public interface AuthToken
	{

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static String safeCast(String key, java.util.Map<String, Object> authToken) throws org.neo4j.kernel.api.security.exception.InvalidAuthTokenException
	//	 {
	//		  Object value = authToken.get(key);
	//		  if (value == null)
	//		  {
	//				throw invalidToken("missing key `" + key + "`");
	//		  }
	//		  else if (!(value instanceof String))
	//		  {
	//				throw invalidToken("the value associated with the key `" + key + "` must be a String but was: " + value.getClass().getSimpleName());
	//		  }
	//		  return (String) value;
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static byte[] safeCastCredentials(String key, java.util.Map<String, Object> authToken) throws org.neo4j.kernel.api.security.exception.InvalidAuthTokenException
	//	 {
	//		  Object value = authToken.get(key);
	//		  if (value == null)
	//		  {
	//				throw invalidToken("missing key `" + key + "`");
	//		  }
	//		  else if (!(value instanceof byte[]))
	//		  {
	//				throw invalidToken("the value associated with the key `" + key + "` must be a UTF-8 encoded string but was: " + value.getClass().getSimpleName());
	//		  }
	//		  return (byte[]) value;
	//	 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") static java.util.Map<String,Object> safeCastMap(String key, java.util.Map<String,Object> authToken) throws org.neo4j.kernel.api.security.exception.InvalidAuthTokenException
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static java.util.Map<String, Object> safeCastMap(String key, java.util.Map<String, Object> authToken) throws org.neo4j.kernel.api.security.exception.InvalidAuthTokenException
	//	 {
	//		  Object value = authToken.get(key);
	//		  if (value == null)
	//		  {
	//				return Collections.emptyMap();
	//		  }
	//		  else if (value instanceof Map)
	//		  {
	//				return (Map<String,Object>) value;
	//		  }
	//		  else
	//		  {
	//				throw new InvalidAuthTokenException("The value associated with the key `" + key + "` must be a Map but was: " + value.getClass().getSimpleName());
	//		  }
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static boolean containsSensitiveInformation(String key)
	//	 {
	//		  return CREDENTIALS.equals(key) || NEW_CREDENTIALS.equals(key);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static void clearCredentials(java.util.Map<String, Object> authToken)
	//	 {
	//		  Object credentials = authToken.get(CREDENTIALS);
	//		  if (credentials instanceof byte[])
	//		  {
	//				Arrays.fill((byte[]) credentials, (byte) 0);
	//		  }
	//
	//		  Object newCredentials = authToken.get(NEW_CREDENTIALS);
	//		  if (newCredentials instanceof byte[])
	//		  {
	//				Arrays.fill((byte[]) newCredentials, (byte) 0);
	//		  }
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static org.neo4j.kernel.api.security.exception.InvalidAuthTokenException invalidToken(String explanation)
	//	 {
	//		  if (StringUtils.isNotEmpty(explanation) && !explanation.matches("^[,.:;].*"))
	//		  {
	//				explanation = ", " + explanation;
	//		  }
	//		  return new InvalidAuthTokenException(format("Unsupported authentication token%s", explanation));
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static java.util.Map<String, Object> newBasicAuthToken(String username, byte[] password)
	//	 {
	//		  return map(AuthToken.SCHEME_KEY, BASIC_SCHEME, AuthToken.PRINCIPAL, username, AuthToken.CREDENTIALS, password);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static java.util.Map<String, Object> newBasicAuthToken(String username, byte[] password, String realm)
	//	 {
	//		  return map(AuthToken.SCHEME_KEY, BASIC_SCHEME, AuthToken.PRINCIPAL, username, AuthToken.CREDENTIALS, password, AuthToken.REALM_KEY, realm);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static java.util.Map<String, Object> newCustomAuthToken(String principle, byte[] credentials, String realm, String scheme)
	//	 {
	//		  return map(AuthToken.SCHEME_KEY, scheme, AuthToken.PRINCIPAL, principle, AuthToken.CREDENTIALS, credentials, AuthToken.REALM_KEY, realm);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static java.util.Map<String, Object> newCustomAuthToken(String principle, byte[] credentials, String realm, String scheme, java.util.Map<String, Object> parameters)
	//	 {
	//		  return map(AuthToken.SCHEME_KEY, scheme, AuthToken.PRINCIPAL, principle, AuthToken.CREDENTIALS, credentials, AuthToken.REALM_KEY, realm, AuthToken.PARAMETERS, parameters);
	//	 }

		 // For testing purposes only
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static java.util.Map<String, Object> newBasicAuthToken(String username, String password)
	//	 {
	//		  return newBasicAuthToken(username, UTF8.encode(password));
	//	 }

		 // For testing purposes only
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static java.util.Map<String, Object> newBasicAuthToken(String username, String password, String realm)
	//	 {
	//		  return newBasicAuthToken(username, UTF8.encode(password), realm);
	//	 }

		 // For testing purposes only
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static java.util.Map<String, Object> newCustomAuthToken(String principle, String credentials, String realm, String scheme)
	//	 {
	//		  return newCustomAuthToken(principle, UTF8.encode(credentials), realm, scheme);
	//	 }

		 // For testing purposes only
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static java.util.Map<String, Object> newCustomAuthToken(String principle, String credentials, String realm, String scheme, java.util.Map<String, Object> parameters)
	//	 {
	//		  return newCustomAuthToken(principle, UTF8.encode(credentials), realm, scheme, parameters);
	//	 }

	}

	public static class AuthToken_Fields
	{
		 public const string SCHEME_KEY = "scheme";
		 public const string PRINCIPAL = "principal";
		 public const string CREDENTIALS = "credentials";
		 public const string REALM_KEY = "realm";
		 public const string PARAMETERS = "parameters";
		 public const string NEW_CREDENTIALS = "new_credentials";
		 public const string BASIC_SCHEME = "basic";
		 public const string NATIVE_REALM = "native";
	}

}