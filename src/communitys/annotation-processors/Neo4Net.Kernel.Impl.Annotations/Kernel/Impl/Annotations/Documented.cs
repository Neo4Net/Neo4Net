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
namespace Neo4Net.Kernel.Impl.Annotations
{

	/// <summary>
	/// Defines documentation for a class, interface, field or method.
	/// 
	/// If no documentation is given for the <seealso cref="value() value"/> to this
	/// annotation, the JavaDoc documentation comment will be
	/// <seealso cref="DocumentationProcessor extracted at compile time"/> and inserted as the
	/// <seealso cref="value() value"/> of this annotation. If no JavaDoc is specified a
	/// compiler warning will be issued.
	/// 
	/// Note that for the JavaDoc to be possible to be extracted it must come before
	/// any annotation on the documented element.
	/// 
	/// Use <seealso cref="DocumentedUtils"/> to extract message from method annotated with <seealso cref="Documented"/>.
	/// 
	/// @author Tobias Ivarsson
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class Documented : System.Attribute
	{
		 internal string value;

		public Documented( String value )
		{
			this.value = value;
		}
	}

}