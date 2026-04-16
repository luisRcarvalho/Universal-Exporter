# Universal Exporter Unity
O **Universal Exporter** é uma ferramenta de editor para a Unity criada para extrair os dados de uma cena ou projeto e organizá-los de forma legível. Ele lê os componentes do seu jogo, converte as informações em arquivos JSON separados por categoria e gera um painel em HTML para que você possa visualizar e conferir esses dados fora da engine.

O objetivo do projeto é facilitar a portabilidade e a leitura de dados do jogo, permitindo que informações estruturais e configurações sejam acessadas sem precisar abrir a Unity.

**Como o projeto está estruturado**
A principal regra na construção deste exportador foi a facilidade de manutenção. O código não depende de um arquivo central gigante cheio de condicionais.

O sistema é dividido em módulos. Cada componente que precisa ser lido (como Física, Câmera ou Áudio) possui um script próprio com uma única responsabilidade. A janela principal da ferramenta busca e lista esses módulos automaticamente. Se o seu projeto precisar exportar um dado novo e específico, basta adicionar um novo script na pasta de módulos e o sistema fará o resto.

**Dados suportados**
O pacote base já consegue mapear e exportar os seguintes elementos:

* **Hierarquia**: A árvore completa de GameObjects da cena e seus componentes.

* **Física**: Configurações globais de gravidade, matriz de colisão, Rigidbodies e Colliders.

* **Iluminação e Câmera**: Propriedades de luzes, reflection probes e configurações de renderização da câmera.

* **Materiais e Áudio**: Cores, caminhos de texturas, shaders e as configurações de AudioSources.

* **Interface (UI)**: A estrutura de Canvases e seus elementos visuais.

* **Animação**: A estrutura completa de Animator Controllers, incluindo camadas, estados, transições e parâmetros.

* **Configurações**: Extração de ScriptableObjects e dados globais do projeto (Quality, Time e Player Settings).

* **Post-Processing**: Volumes da cena e substituições de efeitos.

A ferramenta também rastreia os caminhos dos arquivos físicos (como áudios e texturas) referenciados na cena para garantir que eles não sejam perdidos na exportação.

**O Caso de Uso: Jogo Runner**
Esta ferramenta foi desenvolvida e testada tendo como base uma cena de um jogo estilo Runner. Isso significa que ela foi validada em um cenário real e dinâmico, exigindo a extração correta de sistemas específicos.

Durante o mapeamento do Runner, a ferramenta foi capaz de ler com precisão gerenciadores de instâncias baseados em ScriptableObjects, extrair os parâmetros que definem o "game feel" (como ajustes finos de câmera e pós-processamento) e exportar as máquinas de estado que controlam a movimentação e as reações do jogador.

**Como utilizar**
Importe a pasta do Universal Exporter para dentro do seu projeto Unity.

No menu superior da Unity, acesse Tools > Universal Exporter.

Na janela que se abrirá, selecione quais módulos você deseja executar.

Escolha se deseja exportar apenas a cena atual ou o projeto inteiro.

Selecione a pasta no seu computador onde os arquivos devem ser salvos.
