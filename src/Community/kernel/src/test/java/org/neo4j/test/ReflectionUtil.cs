using System;
using System.Reflection;

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
namespace Neo4Net.Test
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.reflect.FieldUtils.readField;

	public sealed class ReflectionUtil
	{
		 private ReflectionUtil()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <T> T getPrivateField(Object target, String fieldName, Class<T> fieldType) throws Exception
		 public static T GetPrivateField<T>( object target, string fieldName, Type fieldType )
		 {
				 fieldType = typeof( T );
			  return fieldType.cast( readField( target, fieldName, true ) );
		 }

		 public static void VerifyMethodExists( Type owner, string methodName )
		 {
			  foreach ( System.Reflection.MethodInfo method in owner.GetMethods( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance ) )
			  {
					if ( methodName.Equals( method.Name ) )
					{
						 return;
					}
			  }
			  throw new System.ArgumentException( "Method '" + methodName + "' does not exist in class " + owner );
		 }
	}

}