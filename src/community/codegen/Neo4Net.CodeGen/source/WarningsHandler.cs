using System.Collections.Generic;

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
namespace Neo4Net.CodeGen.source
{

	internal interface WarningsHandler
	{

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: void handle(java.util.List<javax.tools.Diagnostic<? extends javax.tools.JavaFileObject>> diagnostics);
		 void handle<T1>( IList<T1> diagnostics );
	}

	public static class WarningsHandler_Fields
	{
		 public static readonly WarningsHandler NoWarningsHandler = diagnostics =>
		 {
		 };

	}

	 internal class WarningsHandler_Multiplex : WarningsHandler
	 {
		  internal readonly WarningsHandler[] Handlers;

		  internal WarningsHandler_Multiplex( params WarningsHandler[] handlers )
		  {
				this.Handlers = handlers;
		  }

		  public override void Handle<T1>( IList<T1> diagnostics ) where T1 : javax.tools.JavaFileObject
		  {
				foreach ( WarningsHandler handler in Handlers )
				{
					 handler.Handle( diagnostics );
				}
		  }
	 }

}