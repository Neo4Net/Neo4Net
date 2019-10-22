/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.impl.store.format.highlimit.v320
{
	using StandardFormatSettings = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings;

	/// <summary>
	/// Reference class for high limit format settings.
	/// </summary>
	/// <seealso cref= HighLimit </seealso>
	public class HighLimitFormatSettingsV3_2_0
	{
		 /// <summary>
		 /// Default maximum number of bits that can be used to represent id
		 /// </summary>
		 internal const int DEFAULT_MAXIMUM_BITS_PER_ID = 50;

		 internal const int PROPERTY_MAXIMUM_ID_BITS = DEFAULT_MAXIMUM_BITS_PER_ID;
		 internal const int NODE_MAXIMUM_ID_BITS = DEFAULT_MAXIMUM_BITS_PER_ID;
		 internal const int RELATIONSHIP_MAXIMUM_ID_BITS = DEFAULT_MAXIMUM_BITS_PER_ID;
		 internal const int RELATIONSHIP_GROUP_MAXIMUM_ID_BITS = DEFAULT_MAXIMUM_BITS_PER_ID;
		 internal const int DYNAMIC_MAXIMUM_ID_BITS = DEFAULT_MAXIMUM_BITS_PER_ID;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") static final int PROPERTY_TOKEN_MAXIMUM_ID_BITS = org.Neo4Net.kernel.impl.store.format.standard.StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS;
		 internal const int PROPERTY_TOKEN_MAXIMUM_ID_BITS = StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") static final int LABEL_TOKEN_MAXIMUM_ID_BITS = org.Neo4Net.kernel.impl.store.format.standard.StandardFormatSettings.LABEL_TOKEN_MAXIMUM_ID_BITS;
		 internal const int LABEL_TOKEN_MAXIMUM_ID_BITS = StandardFormatSettings.LABEL_TOKEN_MAXIMUM_ID_BITS;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") static final int RELATIONSHIP_TYPE_TOKEN_MAXIMUM_ID_BITS = org.Neo4Net.kernel.impl.store.format.standard.StandardFormatSettings.RELATIONSHIP_TYPE_TOKEN_MAXIMUM_ID_BITS;
		 internal const int RELATIONSHIP_TYPE_TOKEN_MAXIMUM_ID_BITS = StandardFormatSettings.RELATIONSHIP_TYPE_TOKEN_MAXIMUM_ID_BITS;

		 private HighLimitFormatSettingsV3_2_0()
		 {
		 }
	}

}