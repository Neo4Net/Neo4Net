﻿using System;
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
namespace Org.Neo4j.Graphdb.config
{

	/// @deprecated The settings API will be completely rewritten in 4.0 
	[Obsolete("The settings API will be completely rewritten in 4.0")]
	public interface SettingValidator
	{
		 /// <summary>
		 /// Validate one or several setting values, throwing on invalid values.
		 /// </summary>
		 /// <param name="settings"> available to be validated </param>
		 /// <param name="warningConsumer"> a consumer for configuration warnings </param>
		 /// <returns> the set of settings considered valid by this validator </returns>
		 /// <exception cref="InvalidSettingException"> if invalid value detected </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.Map<String,String> validate(java.util.Map<String,String> settings, System.Action<String> warningConsumer) throws InvalidSettingException;
		 IDictionary<string, string> Validate( IDictionary<string, string> settings, System.Action<string> warningConsumer );
	}


}