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
namespace Neo4Net.Kernel.Api.Exceptions.schema
{
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using SchemaRule = Neo4Net.Kernel.Api.StorageEngine.schema.SchemaRule;

	public class SchemaRuleNotFoundException : SchemaRuleException
	{
		 private const string NOT_FOUND_MESSAGE_TEMPLATE = "No %s was found for %s.";

		 public SchemaRuleNotFoundException( Neo4Net.Kernel.Api.StorageEngine.schema.SchemaRule_Kind kind, SchemaDescriptor descriptor ) : base( Neo4Net.kernel.api.exceptions.Status_Schema.SchemaRuleAccessFailed, NOT_FOUND_MESSAGE_TEMPLATE, kind, descriptor )
		 {
		 }
	}

}