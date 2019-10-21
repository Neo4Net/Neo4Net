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
namespace Neo4Net.Server.rest
{
	using Neo4Net.Functions;

	public class UniqueStrings
	{
		 private UniqueStrings()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.neo4j.function.Factory<String> withPrefix(final String prefix)
		 public static IFactory<string> WithPrefix( string prefix )
		 {
			  return new FactoryAnonymousInnerClass( prefix );
		 }

		 private class FactoryAnonymousInnerClass : IFactory<string>
		 {
			 private string _prefix;

			 public FactoryAnonymousInnerClass( string prefix )
			 {
				 this._prefix = prefix;
			 }

			 private int next;

			 public string newInstance()
			 {
				  return _prefix + "_" + DateTimeHelper.CurrentUnixTimeMillis() + "_" + ++next;
			 }
		 }

	}

}