﻿using System;

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
namespace Neo4Net.Kernel.Impl.Annotations
{

	/// <summary>
	/// Utility methods for <seealso cref="Documented"/> annotation.
	/// </summary>
	public sealed class DocumentedUtils
	{
		 private DocumentedUtils()
		 {
			  throw new Exception( "Should not be instantiated." );
		 }

		 public static string ExtractMessage( System.Reflection.MethodInfo method )
		 {
			  string message;
			  Documented annotation = method.getAnnotation( typeof( Documented ) );
			  if ( annotation != null && !"".Equals( annotation.value() ) )
			  {
					message = annotation.value();
			  }
			  else
			  {
					message = method.Name;
			  }
			  return message;
		 }

		 public static string ExtractFormattedMessage( System.Reflection.MethodInfo method, object[] args )
		 {
			  return string.format( ExtractMessage( method ), args );
		 }
	}

}