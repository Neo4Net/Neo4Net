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
namespace Neo4Net.Kernel.Impl.Annotations
{
	using StringUtils = org.apache.commons.lang3.StringUtils;


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SupportedSourceVersion(SourceVersion.RELEASE_8) @SupportedAnnotationTypes("org.Neo4Net.kernel.impl.annotations.Documented") public class DocumentationProcessor extends AnnotationProcessor
	public class DocumentationProcessor : AnnotationProcessor
	{
		 protected internal override void Process<T1>( TypeElement annotationType, Element annotated, AnnotationMirror annotation, IDictionary<T1> values ) where T1 : javax.lang.model.element.ExecutableElement
		 {
			  if ( values.Count != 1 )
			  {
					Error( annotated, annotation, "Annotation values don't match the expectation" );
					return;
			  }
			  string value = ( string ) values.Values.GetEnumerator().next().Value;
			  if ( StringUtils.isBlank( value ) )
			  {
					Error( annotated, annotation, "Documentation not available for " + annotated );
			  }
		 }
	}

}