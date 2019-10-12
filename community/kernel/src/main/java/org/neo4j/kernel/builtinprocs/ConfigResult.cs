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
namespace Org.Neo4j.Kernel.builtinprocs
{
	using ConfigValue = Org.Neo4j.Configuration.ConfigValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.StringUtils.EMPTY;

	public class ConfigResult
	{
		 public readonly string Name;
		 public readonly string Description;
		 public readonly string Value;
		 public readonly bool Dynamic;

		 internal ConfigResult( ConfigValue configValue )
		 {
			  this.Name = configValue.Name();
			  this.Description = configValue.Description().orElse(EMPTY);
			  this.Value = configValue.ValueAsString().orElse(EMPTY);
			  this.Dynamic = configValue.Dynamic();
		 }
	}

}