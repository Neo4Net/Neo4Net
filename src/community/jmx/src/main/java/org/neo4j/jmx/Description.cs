using System;

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
namespace Neo4Net.Jmx
{

	/// <summary>
	/// Used to provide JMX documentation to management beans.
	/// 
	/// Annotate the M(X)Bean interface and its methods to provide documentation.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = false, Inherited = false), Obsolete]
	public class Description : System.Attribute
	{
		 // TODO: refactor for localization
		 internal string value;

		 internal int impact;

		public Description( String value, int impact = javax.management.MBeanOperationInfo.UNKNOWN )
		{
			this.value = value;
			this.impact = impact;
		}
	}

}