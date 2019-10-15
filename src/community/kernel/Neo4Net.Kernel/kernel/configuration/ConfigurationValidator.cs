﻿using System.Collections.Generic;

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
namespace Neo4Net.Kernel.configuration
{

	using InvalidSettingException = Neo4Net.Graphdb.config.InvalidSettingException;
	using Log = Neo4Net.Logging.Log;

	/// <summary>
	/// Responsible for validating part of a configuration.
	/// </summary>
	public interface ConfigurationValidator
	{
		 /// <param name="config"> to validated. </param>
		 /// <param name="log"> for logging with messages. </param>
		 /// <returns> a map containing any additional settings to add the the configuration </returns>
		 /// <exception cref="InvalidSettingException"> in case of invalid values. </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: java.util.Map<String,String> validate(@Nonnull Config config, @Nonnull Log log) throws org.neo4j.graphdb.config.InvalidSettingException;
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 IDictionary<string, string> Validate( Config config, Log log );
	}

}